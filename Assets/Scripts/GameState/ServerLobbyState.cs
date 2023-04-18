using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using disguys.ConnectionManagement;
using disguys.Gameplay.GameplayObjects;
using disguys.Gameplay.GameplayObjects.Character;
using disguys.Gameplay.Message;
using disguys.Infrastructure;
using disguys.Looby;
using disguys.Utilities;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using VContainer;
using Random = UnityEngine.Random;

namespace disguys.Gameplay.GameState
{
    /// <summary>
    /// Server specialization of core in-game logic.
    /// </summary>
    [RequireComponent(typeof(NetcodeHooks), typeof(NetworkLobbyState))]
    public class ServerLobbyState : GameStateBehaviour
    {
        [SerializeField] PersistentGameState persistentGameState;

        public override GameState ActiveState
        {
            get { return GameState.Lobby; }
        }

        public bool InitialSpawnDone { get; private set; }

        [SerializeField] private ClientLobbyState m_ClientLobbyState;

        [SerializeField] private NetcodeHooks m_NetcodeHooks;

        [SerializeField] private NetworkLobbyState m_NetworkLobbyState;

        [SerializeField] [Tooltip("Make sure this is included in the NetworkManager's list of prefabs!")]
        private NetworkObject m_PlayerPrefab;
        
        [SerializeField] [Tooltip("A collection of locations for spawning players")]
        private Transform[] m_PlayerSpawnPoints;
        
        private List<Transform> m_PlayerSpawnPointsList = null;
        
        [Inject] ISubscriber<LifeStateChangedEventMessage> m_LifeStateChangedEventMessageSubscriber;
        [Inject] LobbyServiceFacade m_LobbyServiceFacade;
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
            m_PersistentGameState.Reset();
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientConnected;
            NetworkManager.Singleton.SceneManager.OnSynchronizeComplete += OnSynchronizeComplete;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnLoadEventCompleted;

            SessionManager<SessionPlayerData>.Instance.OnSessionStarted();
        }
        
        void OnNetworkDespawn()
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientConnected;
            NetworkManager.Singleton.SceneManager.OnSynchronizeComplete -= OnSynchronizeComplete;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnLoadEventCompleted;
        }

        protected override void OnDestroy()
        {
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
                Debug.Log("On Synchronize Complete");
                SpawnPlayer(clientId, true);
            }
        }

        void OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
        {
            if (!InitialSpawnDone && loadSceneMode == LoadSceneMode.Single)
            {
                InitialSpawnDone = true;
                foreach (var kvp in NetworkManager.Singleton.ConnectedClients)
                {
                    SpawnPlayer(kvp.Key, false);
                }
            }
        }

        private async Task UpdateLobbyLock(bool locked)
        {
            await m_LobbyServiceFacade.UpdateLobbyLockAsync(locked);
        }

        public async void StartGame()
        {
            await UpdateLobbyLock(true);
            
            InitializePlayerState();
            SceneLoaderWrapper.Instance.LoadScene("MainGame", useNetworkSceneManager: true);
        }

        private void Update()
        {
            if (OVRInput.GetDown(OVRInput.Button.Two))
            {
                StartGame();
            }
        }

        void OnClientConnected(ulong clientId)
        {
            if (clientId != NetworkManager.Singleton.LocalClientId)
            {
                // If a client disconnects, check for game over in case all other players are already down
               
            }
        }
        
     

        private void InitializePlayerState()
        {
            foreach (var serverCharacter in PlayerServerCharacter.GetPlayerServerCharacters())
            {
                serverCharacter.InitializeState();
            }
        }

        void SpawnPlayer(ulong clientId, bool lateJoin)
        {
            SessionPlayerData? sessionPlayerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(clientId);
            if (sessionPlayerData.HasValue && sessionPlayerData.Value.HasCharacterSpawned) return;

            // If lobby is closing and waiting to start the game, cancel to allow that new player to join
            if (m_NetworkLobbyState.IsLobbyClosed.Value)
            {
                CancelCloseLobby();
            }

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

            var newPlayer = Instantiate(m_PlayerPrefab, Vector3.zero, Quaternion.identity); // PlayerAvatar
            
            var newPlayerCharacter = newPlayer.GetComponent<ServerCharacter>();

            var physicsTransform = newPlayerCharacter.physicsWrapper.Transform;

            if (spawnPoint != null)
            {
                physicsTransform.SetPositionAndRotation(spawnPoint.position, spawnPoint.rotation);
            }
            
            var networkAvatarGuidStateExists =
                newPlayer.TryGetComponent(out NetworkAvatarGuidState networkAvatarGuidState); //PlayerAvatar에 NetworkAvatarGuidState 컴포넌트를 가져옴
            
            Assert.IsTrue(networkAvatarGuidStateExists,
                $"NetworkCharacterGuidState not found on player avatar!");

            // if reconnecting, set the player's position and rotation to its previous state
            if (lateJoin)
            {
                if (sessionPlayerData is { HasCharacterSpawned: true })
                {
                    physicsTransform.SetPositionAndRotation(sessionPlayerData.Value.PlayerPosition,
                        sessionPlayerData.Value.PlayerRotation);
                }
            }
                   
            var playerNetworkObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId); // NetworkManager에 등록한 PlayerPrefab을 소환하는 단계

            var persistentPlayerExists = playerNetworkObject.TryGetComponent(out PersistentPlayer persistentPlayer);  //persistentPlayer컴포넌트를 가져옴
            
            Assert.IsTrue(persistentPlayerExists,
                $"Matching persistent PersistentPlayer for client {clientId} not found!");

            networkAvatarGuidState.AvatarGuid.Value = persistentPlayer.NetworkAvatarGuidState.AvatarGuid.Value;
            
            if (newPlayer.TryGetComponent(out NetworkNameState networkNameState))
            {
                networkNameState.Name.Value = persistentPlayer.NetworkNameState.Name.Value;
            }
           
            
            newPlayer.SpawnWithOwnership(clientId);
        }

        /// <summary>
        /// Cancels the process of closing the lobby.
        /// </summary>
        void CancelCloseLobby()
        {
            m_NetworkLobbyState.IsLobbyClosed.Value = false;
        }
        
        
#if !UNITY_ANDROID || UNITY_STANDALONE_WIN || UNITY_EDITOR
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(100, 200, 900, 300));

            if (GUILayout.Button("Start Game"))
            {
                StartGame();
            }
            
            GUILayout.EndArea();
        }
#endif
    }
}