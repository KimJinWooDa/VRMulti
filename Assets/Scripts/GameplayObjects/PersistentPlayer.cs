using disguys.ConnectionManagement;
using disguys.Gameplay.GameplayObjects.Character;
using disguys.Utilities;
using Unity.Netcode;
using UnityEngine;

namespace disguys.Gameplay.GameplayObjects
{
    [RequireComponent(typeof(NetworkObject))]
    public class PersistentPlayer : NetworkBehaviour
    {
        ///이 NetworkBehaviour는 Netcode의 GameObject(NetworkManager)에서
        /// "Default Player Prefab"으로 사용되며, 플레이어 연결을 나타냅니다.
        /// 이 NetworkBehaviour는 이 연결의 지속 시간 동안
        /// 유지되는 다른 여러 NetworkBehaviour를 포함하고 있으며, 따라서 씬 간에 유지됩니다.
   
        /// 이 Player 객체를 DontDestroyOnLoad 객체로 명시적으로 표시할 필요는 없으며,
        /// Netcode가 씬 로드 간에 이를 마이그레이션 처리합니다.


        [SerializeField] private PersistentPlayerRuntimeCollection m_PersistentPlayerRuntimeCollection;

        [SerializeField] private NetworkNameState m_NetworkNameState;

        [SerializeField] private NetworkAvatarGuidState m_NetworkAvatarGuidState;

        public NetworkNameState NetworkNameState => m_NetworkNameState;

        public NetworkAvatarGuidState NetworkAvatarGuidState => m_NetworkAvatarGuidState;

        public override void OnNetworkSpawn()
        {
            gameObject.name = "Jin" + OwnerClientId;

            m_PersistentPlayerRuntimeCollection.Add(this);

            if (IsServer)
            {
                var sessionPlayerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(OwnerClientId);
                if (sessionPlayerData.HasValue)
                {
                    var playerData = sessionPlayerData.Value;
                    m_NetworkNameState.Name.Value = playerData.PlayerName;
                    if (playerData.HasCharacterSpawned)
                    {
                        m_NetworkAvatarGuidState.AvatarGuid.Value = playerData.AvatarNetworkGuid;
                    }
                    else
                    {
                        m_NetworkAvatarGuidState.SetRandomAvatar();
                        playerData.AvatarNetworkGuid = m_NetworkAvatarGuidState.AvatarGuid.Value;
                        SessionManager<SessionPlayerData>.Instance.SetPlayerData(OwnerClientId, playerData);
                    }
                }
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            RemovePersistentPlayer();
        }

        public override void OnNetworkDespawn()
        {
            RemovePersistentPlayer();
        }

        void RemovePersistentPlayer()
        {
            m_PersistentPlayerRuntimeCollection.Remove(this);
            if (IsServer)
            {
                var sessionPlayerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(OwnerClientId);
                if (sessionPlayerData.HasValue)
                {
                    var playerData = sessionPlayerData.Value;
                    playerData.PlayerName = m_NetworkNameState.Name.Value;
                    playerData.AvatarNetworkGuid = m_NetworkAvatarGuidState.AvatarGuid.Value;
                    SessionManager<SessionPlayerData>.Instance.SetPlayerData(OwnerClientId, playerData);
                }
            }
        }
    }
}

