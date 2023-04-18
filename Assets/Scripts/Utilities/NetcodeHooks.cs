using System;
using Unity.Netcode;

namespace disguys.Utilities
{
    // 이는 NetworkBehaviour가 될 수 없는 클래스에 유용합니다
    // (예: 전용 서버에서 클라이언트에 존재하지만 서버에서 제거되는 NetworkBehaviour가 있으면
    // NetworkBehaviour 인덱싱이 꼬여서 문제가 발생할 수 있습니다.
    // 이를 해결하기 위해 이 기능이 사용됩니다.
    public class NetcodeHooks : NetworkBehaviour
    {
        public event Action OnNetworkSpawnHook;

        public event Action OnNetworkDespawnHook;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            OnNetworkSpawnHook?.Invoke();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            OnNetworkDespawnHook?.Invoke();
        }
    }
}