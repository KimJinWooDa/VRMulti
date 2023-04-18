using Unity.Netcode;
using UnityEngine;

namespace disguys.Gameplay.GameplayObjects
{
    public enum LifeState
    {
        Alive,
        Dead,
    }
    /// <summary>
    /// 이 MonoBehaviour는 이 객체의 생명 상태를 나타내는
    /// LifeState 유형의 NetworkVariable 하나만 포함합니다.
    /// </summary>
    public class NetworkLifeState : NetworkBehaviour
    {
       [SerializeField]
       NetworkVariable<LifeState> m_LifeState = new NetworkVariable<LifeState>(GameplayObjects.LifeState.Alive);
       
       public NetworkVariable<LifeState> LifeState => m_LifeState;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public NetworkVariable<bool> IsGodMode { get; } = new NetworkVariable<bool>(false);
#endif
    }
}

