using Unity.Netcode;
using UnityEngine;

namespace disguys.Gameplay.GameplayObjects.Character
{
    public enum MovementState
    {
        Idle = 0,
        Walking = 1,
    }

    /// <summary>
    /// 해당 컴포넌트는 서버 측에서 입력에 따라 캐릭터를 이동시키는 역할을 담당합니다.
    /// </summary>
    public class ServerCharacterMovement : NetworkBehaviour
    {
        [SerializeField] Rigidbody m_Rigidbody;
        
        private MovementState m_MovementState;

        private MovementStatus m_PreviousState;

        [SerializeField] private ServerCharacter m_CharLogic;

        public OVRCameraRig cam;
        void Awake()
        {
            //이 NetworkBehavior를 스폰될 때까지 비활성화합니다.
            enabled = false;
        }

        public override void OnNetworkSpawn()
        {
            if (IsServer)
            {
                //이 컴포넌트는 서버에서만 활성화되도록 설정되어 있습니다.
                enabled = true;
            }
        }
        
        public void SetMovementTarget(Vector3 position)
        {
            m_Rigidbody.MovePosition(m_Rigidbody.position + m_Rigidbody.transform.TransformDirection(position) * Time.fixedDeltaTime);//            m_Rigidbody.MovePosition(m_Rigidbody.position + position) * Time.fixedDeltaTime);/

        }

        public void SetRotatoin(float horizontal) //Vector3 rotationVector
        {
            transform.Rotate(0, horizontal * Time.deltaTime, 0);
        }
        

        private void FixedUpdate()
        {
            var currentState = GetMovementStatus(m_MovementState);
            if (m_PreviousState != currentState)
            {
                m_CharLogic.MovementStatus.Value = currentState;
                m_PreviousState = currentState;
            }
  
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                // 객체를 삭제할 때 서버에서 해당 컴포넌트를 비활성화시키는 것입니다.
                // 이렇게 함으로써, 불필요한 리소스 사용을 방지할 수 있습니다.
                enabled = false;
            }
        }
        
        
        /// <summary>
        /// 이 함수는 캐릭터에 적절한 MovementStatus를 결정합니다.
        /// MovementStatus는 클라이언트 코드에서 캐릭터를 애니메이션할 때 사용됩니다.
        /// </summary>
        public MovementStatus GetMovementStatus(MovementState movementState)
        {
            switch (movementState)
            {
                case MovementState.Walking:
                    return MovementStatus.Walking;
                default:
                    return MovementStatus.Idle;
            }
        }
    }
}
