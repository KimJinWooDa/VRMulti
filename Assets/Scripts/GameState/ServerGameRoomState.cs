using System;
using System.Collections;
using System.Collections.Generic;
using disguys.ConnectionManagement;
using disguys.Gameplay.GameplayObjects;
using disguys.Gameplay.GameplayObjects.Character;
using disguys.Gameplay.Message;
using disguys.Infrastructure;
using disguys.Utilities;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using VContainer;
using Random = UnityEngine.Random;
using Avatar = disguys.Gameplay.Configuration.Avatar;

namespace disguys.Gameplay.GameState
{
     /// <summary>
    /// 이것은 GameRoom 게임 로직의 서버 전용 구현입니다.
    /// GameRoom은 다중 플레이어, 협동 전투 요소를 갖춘 게임입니다.
    /// 이 구현은 게임 서버에서 실행되며, 모든 클라이언트의 게임 플레이를 조정합니다.
    /// 서버는 클라이언트의 입력 및 동작을 처리하고,
    /// 게임의 중요한 측면을 동기화하여 공정한 게임 경험을 제공합니다.
    /// 이 GameRoom 서버 전용 구현은 게임 로직과 서버 아키텍처를 결합하여,
    /// 안정적이고 확장 가능한 게임 서버를 만드는 데 사용됩니다.
    /// </summary>
    [RequireComponent(typeof(NetcodeHooks))]
    public class ServerGameRoomState : GameStateBehaviour
    {
        [SerializeField]
        PersistentGameState persistentGameState;

        [SerializeField]
        NetcodeHooks m_NetcodeHooks;

        [SerializeField]
        [Tooltip("Make sure this is included in the NetworkManager's list of prefabs!")]
        private NetworkObject m_PlayerPrefab;

        [SerializeField]
        [Tooltip("A collection of locations for spawning players")]
        private Transform[] m_PlayerSpawnPoints;

        private List<Transform> m_PlayerSpawnPointsList = null;

        public override GameState ActiveState { get { return GameState.MainGame; } }

        // Wait time constants for switching to post game after the game is won or lost
        private const float k_WinDelay = 7.0f;
        private const float k_LoseDelay = 2.5f;

        GameObject m_CurrentCharacterGraphics;
        
        /// <summary>
        /// ServerBossRoomState에서 초기 스폰을 완료했는지를 나타내는 변수입니다.
        /// 즉, 캐릭터 선택에서 게임이 시작된 후 플레이어들을 스폰했는지 여부를 나타냅니다.
        /// 이 값이 true이면 초기 스폰이 이미 완료된 것이고, false이면 아직 초기 스폰이 되지 않았습니다.
        /// </summary>
        public bool InitialSpawnDone { get; private set; }


        /// <summary>
        /// GameState의 수명 동안 구독자를 유지하여, 사라지고 다시 스폰될 때 구독 해제 및 재구독을 허용합니다.
        /// </summary>
        [Inject] ISubscriber<LifeStateChangedEventMessage> m_LifeStateChangedEventMessageSubscriber;
        [Inject] ConnectionManager m_ConnectionManager;
        [Inject] PersistentGameState m_PersistentGameState;

        protected override void Awake()
        {
            base.Awake();
            m_NetcodeHooks.OnNetworkSpawnHook += OnNetworkSpawn;
            m_NetcodeHooks.OnNetworkDespawnHook += OnNetworkDespawn;
        }
        void OnNetworkSpawn()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                enabled = false;
                return;
            }
            //m_NetworkCharacterCreation.LobbyPlayers.OnListChanged += OnLobbyPlayerCharacterStateChanged;
            //m_NetworkCharacterCreation.IsLobbyClosed.OnValueChanged -= OnLobbyClosedChanged;
            m_PersistentGameState.Reset();
            m_LifeStateChangedEventMessageSubscriber.Subscribe(OnLifeStateChangedEventMessage);
            
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;
            NetworkManager.Singleton.SceneManager.OnSynchronizeComplete += OnSynchronizeComplete;
        
            SessionManager<SessionPlayerData>.Instance.OnSessionStarted();
        }

        void OnNetworkDespawn()
        {
            // if (m_NetworkCharacterCreation)
            // {
            //     m_NetworkCharacterCreation.LobbyPlayers.OnListChanged -= OnLobbyPlayerCharacterStateChanged;
            // }
            m_LifeStateChangedEventMessageSubscriber?.Unsubscribe(OnLifeStateChangedEventMessage);
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnect;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
            NetworkManager.Singleton.SceneManager.OnSynchronizeComplete -= OnSynchronizeComplete;
        }

        protected override void OnDestroy()
        {
            m_LifeStateChangedEventMessageSubscriber?.Unsubscribe(OnLifeStateChangedEventMessage);

            if (m_NetcodeHooks)
            {
                m_NetcodeHooks.OnNetworkSpawnHook -= OnNetworkSpawn;
                m_NetcodeHooks.OnNetworkDespawnHook -= OnNetworkDespawn;
            }

            base.OnDestroy();
        }

        void OnSynchronizeComplete(ulong clientId)
        {
            if (InitialSpawnDone && !PlayerServerCharacter.GetPlayerServerCharacter(clientId))
            {
                //somebody joined after the initial spawn. This is a Late Join scenario. This player may have issues
                //(either because multiple people are late-joining at once, or because some dynamic entities are
                //getting spawned while joining. But that's not something we can fully address by changes in
                //ServerBossRoomState.
                //SpawnPlayer(clientId, true);
            }
        }

        void OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            if (!InitialSpawnDone && loadSceneMode == LoadSceneMode.Single)
            {
                InitialSpawnDone = true;
                foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
                {
                    //SpawnPlayer(kvp.Key, false);
                }
            }
        }

        void OnClientDisconnect(ulong clientId)
        {
            if (clientId != NetworkManager.Singleton.LocalClientId)
            {
                // If a client disconnects, check for game over in case all other players are already down
                StartCoroutine(WaitToCheckForGameOver());
            }
        }

        IEnumerator WaitToCheckForGameOver()
        {
            // Wait until next frame so that the client's player character has despawned
            yield return null;
            CheckForGameOver();
        }

        void SpawnPlayer(ulong clientId, bool lateJoin)
        {
            Transform spawnPoint = null;

            if (m_PlayerSpawnPointsList == null || m_PlayerSpawnPointsList.Count == 0)
            {
                m_PlayerSpawnPointsList = new List<Transform>(m_PlayerSpawnPoints);
            }

            Debug.Assert(m_PlayerSpawnPointsList.Count > 0,
                $"PlayerSpawnPoints array should have at least 1 spawn points.");

            int index = Random.Range(0, m_PlayerSpawnPointsList.Count);
            spawnPoint = m_PlayerSpawnPointsList[index];
            m_PlayerSpawnPointsList.RemoveAt(index);

            var playerNetworkObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);

            var newPlayer = Instantiate(m_PlayerPrefab, Vector3.zero, Quaternion.identity);
    
            var newPlayerCharacter = newPlayer.GetComponent<ServerCharacter>();

            var physicsTransform = newPlayerCharacter.physicsWrapper.Transform;

            if (spawnPoint != null)
            {
                physicsTransform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            }

            var persistentPlayerExists = playerNetworkObject.TryGetComponent(out PersistentPlayer persistentPlayer);
            Assert.IsTrue(persistentPlayerExists,
                $"Matching persistent PersistentPlayer for client {clientId} not found!");

            // pass character type from persistent player to avatar
            var networkAvatarGuidStateExists =
                newPlayer.TryGetComponent(out NetworkAvatarGuidState networkAvatarGuidState);

            Assert.IsTrue(networkAvatarGuidStateExists,
                $"NetworkCharacterGuidState not found on player avatar!");

            // if reconnecting, set the player's position and rotation to its previous state
            if (lateJoin)
            {
                SessionPlayerData? sessionPlayerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(clientId);
                if (sessionPlayerData is { HasCharacterSpawned: true })
                {
                    physicsTransform.SetPositionAndRotation(sessionPlayerData.Value.PlayerPosition, sessionPlayerData.Value.PlayerRotation);
                }
            }

            networkAvatarGuidState.AvatarGuid.Value =
                persistentPlayer.NetworkAvatarGuidState.AvatarGuid.Value;

            // pass name from persistent player to avatar
            if (newPlayer.TryGetComponent(out NetworkNameState networkNameState))
            {
                networkNameState.Name.Value = persistentPlayer.NetworkNameState.Name.Value;
            }

            // spawn players characters with destroyWithScene = true
            newPlayer.SpawnWithOwnership(clientId, true);
        }

        void OnLifeStateChangedEventMessage(LifeStateChangedEventMessage message)
        {
            switch (message.CharacterType)
            {
                case CharacterTypeEnum.player:
                    // Every time a player's life state changes to fainted we check to see if game is over
                    if (message.NewLifeState == LifeState.Dead)
                    {
                        CheckForGameOver();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        void CheckForGameOver()
        {
            var alivePlayerCount = PlayerServerCharacter.GetPlayerServerCharacters().FindAll(x => x.Alive);
            if (alivePlayerCount.Count == 1)
            {
                StartCoroutine(CoroGameOver(k_LoseDelay, false));
            }
        }

        void BossDefeated()
        {
            // Boss is dead - set game won to true
            StartCoroutine(CoroGameOver(k_WinDelay, true));
        }

        IEnumerator CoroGameOver(float wait, bool gameWon)
        {
            m_PersistentGameState.SetWinState(gameWon ? WinState.Win : WinState.Loss);

            // wait 5 seconds for game animations to finish
            yield return new WaitForSeconds(wait);

            SceneLoaderWrapper.Instance.LoadScene("MainMenu", useNetworkSceneManager: true);
        }
        
        
        
        Dictionary<Guid, GameObject> m_SpawnedCharacterGraphics = new Dictionary<Guid, GameObject>();

        GameObject ReturnCharacterGraphics(Avatar avatar)
        {
            if (!m_SpawnedCharacterGraphics.TryGetValue(avatar.Guid, out GameObject characterGraphics))
            {
                characterGraphics = Instantiate(avatar.Graphics); //m_CharacterGraphicsParent
                m_SpawnedCharacterGraphics.Add(avatar.Guid, characterGraphics);
            }

            return characterGraphics;
        }
        
 

    }
}