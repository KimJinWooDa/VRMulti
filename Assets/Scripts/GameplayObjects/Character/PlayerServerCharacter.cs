using System.Collections.Generic;
using disguys.ConnectionManagement;
using Unity.Netcode;
using UnityEngine;

namespace disguys.Gameplay.GameplayObjects.Character
{
    /// <summary>
    /// 이 스크립트는 플레이어 캐릭터의 프리팹에 부착되어,
    /// 각 플레이어에 대한 활성화된 ServerCharacter 객체의 목록을 유지합니다.
    /// 이것은 성능 최적화를 위한 것입니다.
    /// 서버 코드에서 이미 플레이어들의 ServerCharacter 목록을 활성 연결을 반복하고 GetComponent()을 호출하여
    /// 얻을 수 있습니다. 하지만 플레이어 목록을 자주 반복해야 하는데,
    /// 이 때 모든 GetComponent() 호출은 누적되어 성능에 영향을 미칩니다.
    /// 따라서 이 최적화는 GetComponent()를 호출하지 않고도 반복할 수 있게 합니다.
    /// 이는 ScriptableObject 기반의 플레이어 컬렉션으로 재구성될 것입니다.
    [RequireComponent(typeof(ServerCharacter))]
    public class PlayerServerCharacter : NetworkBehaviour
    {
        static List<ServerCharacter> s_ActivePlayers = new List<ServerCharacter>();

        [SerializeField]
        ServerCharacter m_CachedServerCharacter;

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                s_ActivePlayers.Add(m_CachedServerCharacter);
            }
            else
            {
                enabled = false;
            }

        }

        void OnDisable()
        {
            s_ActivePlayers.Remove(m_CachedServerCharacter);
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                var movementTransform = m_CachedServerCharacter.Movement.transform;
                SessionPlayerData? sessionPlayerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(OwnerClientId);
                if (sessionPlayerData.HasValue)
                {
                    var playerData = sessionPlayerData.Value;
                    playerData.PlayerPosition = movementTransform.position;
                    playerData.PlayerRotation = movementTransform.rotation;
                    playerData.Alive = m_CachedServerCharacter.Alive;
                    playerData.HasCharacterSpawned = true;
                    SessionManager<SessionPlayerData>.Instance.SetPlayerData(OwnerClientId, playerData);
                }
            }
        }

        /// <summary>
        /// 모든 활성 플레이어의 ServerCharacter 목록을 반환합니다.
        /// 이 목록은 읽기 전용으로 다루어야 합니다. 클라이언트에서는 이 목록이 비어있습니다.
        public static List<ServerCharacter> GetPlayerServerCharacters()
        {
            return s_ActivePlayers;
        }

        /// <summary>
        /// 특정 클라이언트가 소유한 ServerCharacter를 반환합니다.
        public static ServerCharacter GetPlayerServerCharacter(ulong ownerClientId)
        {
            foreach (var playerServerCharacter in s_ActivePlayers)
            {
                if (playerServerCharacter.OwnerClientId == ownerClientId)
                {
                    return playerServerCharacter;
                }
            }
            return null;
        }
    }
}
