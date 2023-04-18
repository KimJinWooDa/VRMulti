using disguys.Gameplay.GameplayObjects.Character;
using disguys.Gameplay.GameState;
using disguys.Utilities;
using Unity.Netcode;
using UnityEngine;
using VContainer;

namespace disguys.Gameplay.UI
{
    [DefaultExecutionOrder(300)]
    public class UIStateDisplayHandler : NetworkBehaviour
    {
        [SerializeField]
        bool m_DisplayName;

        [SerializeField]
        UIStateDisplay m_UIStatePrefab;

        UIStateDisplay m_UIState;

        RectTransform m_UIStateRectTransform;

        bool m_UIStateActive;

        [SerializeField]
        NetworkNameState m_NetworkNameState;

        ServerCharacter m_ServerCharacter;

        ClientAvatarGuidHandler m_ClientAvatarGuidHandler;

        [Tooltip("UI object(s) will appear positioned at this transforms position.")]
        [SerializeField]
        Transform m_TransformToTrack;
        
        Transform m_CanvasTransform;

        [Tooltip("World space vertical offset for positioning.")]
        [SerializeField]
        float m_VerticalWorldOffset;
        
        Vector3 m_VerticalOffset;

        // used to compute world position based on target and offsets
        Vector3 m_WorldPos;

        private Transform m_CameraTransform;


        void Awake()
        {
            m_ServerCharacter = GetComponent<ServerCharacter>();
        }

        public override void OnNetworkSpawn()
        {
            if (!NetworkManager.Singleton.IsClient)
            {
                enabled = false;
                return;
            }

            if (TryGetComponent(out m_ClientAvatarGuidHandler))
            {
                if (m_ServerCharacter.clientCharacter)
                {
                    TrackGraphicsTransform(m_ServerCharacter.clientCharacter.gameObject);
                }
                else
                {
                    m_ClientAvatarGuidHandler.AvatarGraphicsSpawned += TrackGraphicsTransform;
                }

            }

            if (m_DisplayName)
            {
                DisplayUIName();
            }

        }

        void OnDisable()
        {
            if (m_ClientAvatarGuidHandler)
            {
                m_ClientAvatarGuidHandler.AvatarGraphicsSpawned -= TrackGraphicsTransform;
            }
        }

        void DisplayUIName()
        {
            if (m_NetworkNameState == null)
            {
                return;
            }

            if (m_UIState == null)
            {
                SpawnUIState();
            }

            m_UIState.DisplayName(m_NetworkNameState.Name);
            m_UIStateActive = true;
        }
        
        void SpawnUIState()
        {
            m_UIState = Instantiate(m_UIStatePrefab, m_CanvasTransform);
            // make in world UI state draw under other UI elements
            m_UIState.transform.SetAsFirstSibling();
            m_UIStateRectTransform = m_UIState.GetComponent<RectTransform>();
        }

        void TrackGraphicsTransform(GameObject graphicsGameObject)
        {
            m_TransformToTrack = graphicsGameObject.transform;
        }


        void LateUpdate()
        {
            //lobby가 아닌 inGame에서는 닉네임이 다 사라지도록!!!
            if (!m_UIStateRectTransform) return;
         
            if (m_UIStateActive && m_TransformToTrack)
            {
                var position = m_TransformToTrack.position;
                m_WorldPos.Set(position.x,
                    position.y + m_VerticalWorldOffset,
                    position.z);

                m_UIStateRectTransform.position = m_WorldPos;
                //m_UIStateRectTransform.LookAt(CameraTransform, Vector3.up);
                var localEulerAngles = m_UIStateRectTransform.localEulerAngles;
                m_UIStateRectTransform.Rotate(-localEulerAngles.x, 0, -localEulerAngles.z, Space.Self);
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (m_UIState != null)
            {
                Destroy(m_UIState.gameObject);
            }
        }
    }

}
