using Unity.Netcode;

namespace disguys.Gameplay.Message
{
    public struct DoorStateChangedEventMessage : INetworkSerializeByMemcpy
    {
        public bool IsDoorOpen;
    }
}