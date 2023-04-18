using System;
using disguys.Gameplay.Configuration;
using disguys.Utilities;
using Unity.Netcode;
using UnityEngine;

namespace disguys.Gameplay.GameplayObjects.Character
{
    /// <summary>
    /// <see cref="ClientCharacter"/>는 서버에서 보낸 상태 정보를 기반으로
    /// 클라이언트 화면에 캐릭터를 표시하는 역할을 담당합니다.
    /// </summary>
    public class ClientCharacter : NetworkBehaviour
    {
        [SerializeField]
        Animator m_ClientVisualsAnimator;

        [SerializeField]
        VisualizationConfiguration m_VisualizationConfiguration;

        public Animator OurAnimator => m_ClientVisualsAnimator;
        
        ServerCharacter m_ServerCharacter;
        
        PositionLerper m_PositionLerper;

        RotationLerper m_RotationLerper;

        PhysicsWrapper m_PhysicsWrapper;

        // 이 값은 위치 및 회전 보간 모두에 적합합니다. 즉, 각 보간마다 고유한 상수 값을 가질 필요 없이,
        // 이 값 하나로 모든 보간에 대해 일관된 결과를 얻을 수 있습니다.
        const float k_LerpTime = 0.08f;

        Vector3 m_LerpedPosition;

        Quaternion m_LerpedRotation;

        bool m_IsHost;

        void Awake()
        {
            enabled = false;
        }

        public override void OnNetworkSpawn()
        {
            if (!IsClient || transform.parent == null)
            {
                return;
            }

            enabled = true;

            m_IsHost = IsHost;

            m_ServerCharacter = GetComponentInParent<ServerCharacter>();

            m_PhysicsWrapper = m_ServerCharacter.GetComponent<PhysicsWrapper>();

            // m_ServerCharacter.MovementStatus.OnValueChanged += OnMovementStatusChanged;
            // OnMovementStatusChanged(MovementStatus.Idle);

            //서버로부터 받은 가장 최신 버전의 위치와 회전값을 시각화(position & rotation)에 동기화하는 것을 의미
            //즉, 서버에서 계산된 위치와 회전값을 클라이언트의 시각화에도 반영해주는 것으로,
            //다른 플레이어의 위치 및 회전값 등의 변경 사항을 실시간으로 반영하기 위한 과정입니다.
            //이를 통해 다른 플레이어와의 상호작용 등이 가능해지며, 게임의 동기화가 유지됩니다.
            transform.SetPositionAndRotation(m_PhysicsWrapper.Transform.position, m_PhysicsWrapper.Transform.rotation);
            m_LerpedPosition = transform.position;
            m_LerpedRotation = transform.rotation;


            // 해당 코드에서는 부드러운 보간 효과를 위해 시작 위치와 회전 값을 초기화하는 작업이 수행됩니다.
            // 이를 통해 서버로부터 수신한 가장 최신의 위치와 회전 값을 기준으로 보간하는 작업이 가능해지며,
            // 이를 통해 원활한 네트워크 상호작용을 구현할 수 있습니다.
            // 따라서 서버와 클라이언트 간 위치와 회전 값의 동기화를 위한 작업 중 하나로 이해할 수 있습니다.
            m_PositionLerper = new PositionLerper(m_PhysicsWrapper.Transform.position, k_LerpTime);
            m_RotationLerper = new RotationLerper(m_PhysicsWrapper.Transform.rotation, k_LerpTime);
            
            if (m_ServerCharacter.TryGetComponent(out ClientAvatarGuidHandler clientAvatarGuidHandler))
            {
                m_ClientVisualsAnimator = clientAvatarGuidHandler.graphicsAnimator;
            }
        }

        public override void OnNetworkDespawn()
        {
            enabled = false;
        }

        
        
        /// <summary>
        /// 이 함수는 현재 게임 플레이 조건에 따라 Animator의 "Speed" 변수에 설정해야 하는 값을 반환합니다.
        /// Animator 컴포넌트는 애니메이션의 재생 속도를 제어하는 변수인 "Speed"를 가지고 있습니다.
        /// 이 변수에 값을 설정하면 애니메이션의 재생 속도가 조절됩니다.
        /// 이 함수는 게임 플레이 조건에 따라서 애니메이션의 속도를 적절히 조절하기 위해 호출됩니다.
        float GetVisualMovementSpeed(MovementStatus movementStatus)
        {
            if (m_ServerCharacter.NetLifeState.LifeState.Value != LifeState.Alive)
            {
                return 0;
            }

            switch (movementStatus)
            {
                case MovementStatus.Idle:
                    return m_VisualizationConfiguration.Idle;
                case MovementStatus.Walking:
                    return m_VisualizationConfiguration.SpeedWalking;
                default:
                    throw new Exception($"Unknown MovementStatus {movementStatus}");
            }
        }
        
        void FixedUpdate()
        {
            if (m_IsHost)
            {
                //주의: 캐시 된 위치(m_LerpedPosition)와 회전(m_LerpedRotation)은 각 보간의 시작점으로 사용되며,
                //FixedUpdate에서 루트의 위치와 회전이 수정되므로(transform이 자식인 이 transform도 수정됨) 이 transform이 변경됩니다.
                m_LerpedPosition = m_PositionLerper.LerpPosition(m_LerpedPosition,
                    m_PhysicsWrapper.Transform.position);
                m_LerpedRotation = m_RotationLerper.LerpRotation(m_LerpedRotation,
                    m_PhysicsWrapper.Transform.rotation);
                transform.SetPositionAndRotation(m_LerpedPosition, m_LerpedRotation);
            }
            
                      
            if (m_ClientVisualsAnimator)
            {
                OurAnimator.SetFloat(m_VisualizationConfiguration.SpeedVariableID, GetVisualMovementSpeed(m_ServerCharacter.MovementStatus.Value));
            }
        }
    }
}
