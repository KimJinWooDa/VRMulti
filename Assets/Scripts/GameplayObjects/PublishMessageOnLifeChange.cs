using disguys.Gameplay.GameplayObjects.Character;
using disguys.Gameplay.GameState;
using disguys.Gameplay.Message;
using disguys.Infrastructure;
using disguys.Utilities;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace disguys.Gameplay.GameplayObjects
{
  
    /// <summary>
    /// LifeState가 변경될 때 메시지를 게시하는 서버 전용 컴포넌트입니다.
    /// </summary>
    [RequireComponent(typeof(NetworkLifeState), typeof(ServerCharacter))]
    public class PublishMessageOnLifeChange : NetworkBehaviour
    {
        NetworkLifeState m_NetworkLifeState;
        ServerCharacter m_ServerCharacter;

        [SerializeField]
        string m_CharacterName;

        NetworkNameState m_NameState;

        [Inject]
        IPublisher<LifeStateChangedEventMessage> m_Publisher;

        void Awake()
        {
            m_NetworkLifeState = GetComponent<NetworkLifeState>();
            m_ServerCharacter = GetComponent<ServerCharacter>();
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                m_NameState = GetComponent<NetworkNameState>();
                m_NetworkLifeState.LifeState.OnValueChanged += OnLifeStateChanged;

                var gameState = FindObjectOfType<ServerGameRoomState>();
                if (gameState != null)
                {
                    gameState.Container.Inject(this);
                }
            }
        }

        void OnLifeStateChanged(LifeState previousState, LifeState newState)
        {
            m_Publisher.Publish(new LifeStateChangedEventMessage()
            {
                CharacterName = m_NameState != null ? m_NameState.Name.Value : (FixedPlayerName)m_CharacterName,
                //CharacterType = m_ServerCharacter.CharacterClass.CharacterType,
                NewLifeState = newState
            });
        }
    }
}
