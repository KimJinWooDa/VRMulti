using Unity.Netcode;


namespace disguys.Utilities
{
    /// 특정 인스턴스의 씬 로딩 진행 상황을 추적하는 간단한 객체
    public class NetworkedLoadingProgressTracker : NetworkBehaviour
    {
        /// 이 NetworkBehavior의 소유자와 관련된 현재 로딩 진행 상황
        public NetworkVariable<float> Progress { get; } = new NetworkVariable<float>(0,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    }
}



