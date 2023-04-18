using UnityEngine;
using UnityEngine.Serialization;

namespace disguys.Gameplay.Configuration
{
    /// <summary>
    /// 이 클래스는 특정 캐릭터의 시각화가 어떻게 애니메이션화될지 설명합니다.
    /// 이는 애니메이션의 이름, 애니메이션의 재생 속도, 애니메이션의 가중치 등을 지정하는 데 사용됩니다.
    /// 이를 통해 캐릭터가 움직이는 방식에 따라 적절한 애니메이션을 재생할 수 있습니다.
    /// </summary>
    [CreateAssetMenu]
    public class VisualizationConfiguration : ScriptableObject
    {
        [SerializeField] string m_DeadStateTrigger = "Dead";
        [SerializeField] string m_SpeedVariable = "Speed";
        [SerializeField] string m_AttackTrigger = "Attack";
        
        [FormerlySerializedAs("SpeedIdle")]
        [Header("Animation Speeds")]
        public float Idle = 0;
        public float SpeedWalking = 1f;
        

        // These are maintained by our OnValidate(). Code refers to these hashed values, not the string versions!
        [HideInInspector] public int DeadStateTriggerID;
        [HideInInspector] public int SpeedVariableID;
        [HideInInspector] public int AttackVariableID;

        void OnValidate()
        {
            DeadStateTriggerID = Animator.StringToHash(m_DeadStateTrigger);

            SpeedVariableID = Animator.StringToHash(m_SpeedVariable);

            AttackVariableID = Animator.StringToHash(m_AttackTrigger);
        }
    }

}
