namespace disguys.Gameplay.GameState
{
    public enum WinState
    {
        Invalid,
        Win,
        Loss
    }

    /// <summary>
    /// ServerBossRoomState와 PostGameState 간의 게임 세션의 승리 상태를 나타내기 위해 전달되어야 하는 일부 데이터를 포함하는 클래스입니다.
    /// </summary>
    public class PersistentGameState
    {
        public WinState WinState { get; private set; }

        public void SetWinState(WinState winState)
        {
            WinState = winState;
        }

        public void Reset()
        {
            WinState = WinState.Invalid;
        }
    }
}