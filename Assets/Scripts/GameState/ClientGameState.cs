namespace disguys.Gameplay.GameState
{
    public class ClientGameState : GameStateBehaviour
    {
        public override GameState ActiveState
        {
            get { return GameState.MainGame; }
        }  
    }
}