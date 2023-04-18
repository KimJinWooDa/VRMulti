using disguys.Gameplay.GameplayObjects;
using disguys.Gameplay.GameplayObjects.Character;
using disguys.Utilities;
using Unity.Netcode;

namespace disguys.Gameplay.Message
{
    public struct LifeStateChangedEventMessage : INetworkSerializeByMemcpy
    {
        public LifeState NewLifeState;
        public CharacterTypeEnum CharacterType;
        public FixedPlayerName CharacterName;
    }
}

