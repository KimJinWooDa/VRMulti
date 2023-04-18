using Unity.Netcode;

namespace disguys.Gameplay.GameState
{
    public class NetworkLobbyState : NetworkBehaviour
    {
        /// <summary>
        /// 이 값이 true가 되면, 로비가 닫히고 게임 플레이로 전환됩니다
        /// </summary>
        public NetworkVariable<bool> IsLobbyClosed { get; } = new NetworkVariable<bool>(false);
    }
}