using disguys.Infrastructure;
using UnityEngine;

namespace disguys.ConnectionManagement
{
    public struct SessionPlayerData : ISessionPlayerData
    {
        public string PlayerName;
        public int PlayerNumber;
        public Vector3 PlayerPosition;
        public Quaternion PlayerRotation;
        public NetworkGuid AvatarNetworkGuid;
        public bool Alive;
        public bool HasCharacterSpawned;

        public SessionPlayerData(ulong clientID, string name, NetworkGuid avatarNetworkGuid, bool alive = true, bool isConnected = false, bool hasCharacterSpawned = false)
        {
            ClientID = clientID;
            PlayerName = name;
            PlayerNumber = -1;
            PlayerPosition = Vector3.zero;
            PlayerRotation = Quaternion.identity;
            AvatarNetworkGuid = avatarNetworkGuid;
            Alive = alive;
            IsConnected = isConnected;
            HasCharacterSpawned = hasCharacterSpawned;
        }
        
        public bool IsConnected { get; set; }
        public ulong ClientID { get; set; }
        public void Reinitialize()
        {
            HasCharacterSpawned = false;
        }
    }

}
