using System;
using System.Collections;
using disguys.ConnectionManagement;
using Unity.Netcode;
using UnityEngine;

namespace disguys.Gameplay.GameplayObjects.Character
{
    /// <summary>
    /// 이 클래스는 캐릭터의 모든 NetworkVariable, RPC 및 서버 측 로직을 포함하고 있습니다.
    /// 클라이언트 및 서버 컨텍스트를 자체 포함하여 이 클래스를 두 부분으로 분리했습니다.
    /// 이렇게하면 코드가 클라이언트 측 또는 서버 측에서 실행되는지 계속해서 확인할 필요가 없습니다.
    /// </summary>
    [RequireComponent(typeof(NetworkLifeState),
        typeof(NetworkHealthState),
        typeof(NetworkAvatarGuidState))]
    public class ServerCharacter : NetworkBehaviour
    {
        [SerializeField] ClientCharacter m_ClientCharacter;
        public ClientCharacter clientCharacter => m_ClientCharacter;
        
        public NetworkVariable<MovementStatus> MovementStatus { get; } = new NetworkVariable<MovementStatus>(GameplayObjects.MovementStatus.Idle, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        
        public NetworkHealthState NetAliveState { get; private set; }

        /// <summary>
        /// 현재 체력(여기선 상태 -> bool값)
        /// 이 값은 캐릭터 클래스 데이터에서 시작 시간에 채워집니다.
        /// </summary>
        public bool Alive
        {
            get => NetAliveState.Alive.Value;
            private set => NetAliveState.Alive.Value = value;
        }
        
        public NetworkLifeState NetLifeState { get; private set; }

        public LifeState LifeState
        {
            get => NetLifeState.LifeState.Value;
            private set => NetLifeState.LifeState.Value = value;
        }
        
        public void InitializeState()
        {
            LifeState = LifeState.Alive;
        }
        
        //어떤 객체가 파괴될 때 자동으로 제거되는 것을 비활성화하려면 음수 값을 설정합니다.
        [SerializeField] [Tooltip("Setting negative value disables destroying object after it is killed.")]
        private float m_KilledDestroyDelaySeconds = 3.0f;

        [SerializeField] DamageReceiver m_DamageReceiver;

        [SerializeField] ServerCharacterMovement m_Movement;

        public ServerCharacterMovement Movement => m_Movement;

        [SerializeField] PhysicsWrapper m_PhysicsWrapper;

        public PhysicsWrapper physicsWrapper => m_PhysicsWrapper;

        [SerializeField] ServerAnimationHandler m_ServerAnimationHandler;

        public ServerAnimationHandler serverAnimationHandler => m_ServerAnimationHandler;

        //NetworkAvatarGuidState m_State;

        private void Awake()
        {
            NetLifeState = GetComponent<NetworkLifeState>();
            NetAliveState = GetComponent<NetworkHealthState>();
            //m_State = GetComponent<NetworkAvatarGuidState>();
        }

        public override void OnNetworkSpawn()
        {
            if (!IsServer)
            {
                enabled = false;
            }
            else
            {
                
               // this.gameObject.AddComponent<CameraController>();
                NetLifeState.LifeState.OnValueChanged += OnLifeStateChanged;
                m_DamageReceiver.DamageReceived += GetHit;
                m_DamageReceiver.CollisionEntered += CollisionEntered;
                InitializeSurviveState();
            }
        }

        public override void OnNetworkDespawn()
        {
            NetLifeState.LifeState.OnValueChanged -= OnLifeStateChanged;

            if (m_DamageReceiver)
            {
                m_DamageReceiver.DamageReceived -= GetHit;
                m_DamageReceiver.CollisionEntered -= CollisionEntered;
            }
        }

     
        /// <summary>
        /// 클라이언트에서 서버로 이 캐릭터의 입력을 보내는 RPC입니다.
        /// movementTarget 매개변수는 이 캐릭터가 이동해야 할 위치입니다.
        /// </summary>
        [ServerRpc]
        public void SendCharacterInputServerRpc(Vector3 movementTarget)
        {
            if (LifeState == LifeState.Alive)
            {
                m_Movement.SetMovementTarget(movementTarget);
            }
        }

        [ServerRpc]
        public void SendCharacterRotationInputServerRpc(float rot) //Vector3 rotationVector
        {
            if (LifeState == LifeState.Alive)
            {
                m_Movement.SetRotatoin(rot);
            }
        }

        void InitializeSurviveState()
        {
            if (Alive == false)
            {
                Alive = true;
            }

            SessionPlayerData? sessionPlayerData =
                SessionManager<SessionPlayerData>.Instance.GetPlayerData(OwnerClientId);
            if (sessionPlayerData is { HasCharacterSpawned: true })
            {
                Alive = sessionPlayerData.Value.Alive;
                if (Alive == false)
                {
                    LifeState = LifeState.Dead;
                }
            }

        }

        void OnLifeStateChanged(LifeState prevLifeState, LifeState lifeState)
        {
            if (lifeState != LifeState.Alive)
            {
                m_Movement.SetMovementTarget(Vector3.zero);
            }
        }
        
        IEnumerator KilledDestroyProcess()
        {
            yield return new WaitForSeconds(m_KilledDestroyDelaySeconds);

            if (NetworkObject != null)
            {
                NetworkObject.Despawn(true);
            }
        }
        
 
        public void GetHit(ServerCharacter inflicter, bool GetHit)
        {
            //나중에 Hit한 사람의 이름을 받기 위해 inflicter지우지말기
            
            //이 게임에서는, 피해를 바로 받습니다.
            //다른 게임에서는 해당 피해나 치유에 대해 본인 게임만의 효과를 적용하고,
            //필요에 따라 피해나 치유를 수정합니다.
            if(GetHit && Alive) //공격당할 때
            {
                StartCoroutine(KilledDestroyProcess());
                LifeState = LifeState.Dead;
                //serverAnimationHandler.NetworkAnimator.SetTrigger("Dead");
            }

            Alive = false;
        }
        
        void CollisionEntered(Collision collision)
        {
          
        }

       
    }
}


