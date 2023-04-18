using Unity.Netcode;
using UnityEngine;

namespace disguys.Gameplay.GameplayObjects.Character
{
    public class NetworkAvatarState : NetworkBehaviour
    {
        [HideInInspector]
        public NetworkVariable<ulong> AvatarID = new NetworkVariable<ulong>();
    }
}