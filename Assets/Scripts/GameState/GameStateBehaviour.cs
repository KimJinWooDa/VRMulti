using UnityEngine;
using VContainer.Unity;

namespace disguys.Gameplay.GameState
{
    public enum GameState
    {
        MainMenu,
        Lobby,
        MainGame,
        PostGame
    }

    /// <summary>
    /// GameStateBehaviour는 특정 게임 상태와 그에 필요한 의존성을 나타내는 특별한 컴포넌트입니다.
    /// 이 컴포넌트는 하나의 GameState만이 동시에 실행될 것을 보장하는 특별한 기능을 제공합니다.

    ///주의할 점으로, 모든 Scene에는 GameState 객체가 있다고 가정합니다.
    /// 그렇지 않으면 Persisting game state가 수명을 초과할 수 있습니다.
    /// 이는 해당 상태를 정리할 다음 상태가 없기 때문입니다.

    /// GameStateBehaviour는 서버와 클라이언트 각각 하나씩 있으며,
    /// Persist 속성을 true로 설정하면 다른 Scene으로 전환해도 현재 GameState 객체가 살아남습니다.
    /// 다음 Scene에도 GameState가 있는 경우에는 새로운 GameState가 자동으로 삭제되어 빈 자리를 만듭니다.
    /// GameState와 Scene 간의 관계는 1대 다의 관계이며,
    /// GameStateBehaviours는 모노비헤이비어를 상속하므로,
    /// 하나의 상태가 여러 Scene에 걸쳐 존재할 수 있습니다.
    public abstract class GameStateBehaviour : LifetimeScope
    {
        /// <summary>
        /// Does this GameState persist across multiple scenes?
        /// </summary>
        public virtual bool Persists
        {
            get { return false; }
        }

        /// <summary>
        /// What GameState this represents. Server and client specializations of a state should always return the same enum.
        /// </summary>
        public abstract GameState ActiveState { get; }

        /// <summary>
        /// This is the single active GameState object. There can be only one.
        /// </summary>
        private static GameObject s_ActiveStateGO;

        protected override void Awake()
        {
            base.Awake();

            if (Parent != null)
            {
                Parent.Container.Inject(this);
            }
        }

        // Start is called before the first frame update
        protected virtual void Start()
        {
            if (s_ActiveStateGO != null)
            {
                if (s_ActiveStateGO == gameObject)
                {
                    //nothing to do here, if we're already the active state object.
                    return;
                }

                //on the host, this might return either the client or server version, but it doesn't matter which;
                //we are only curious about its type, and its persist state.
                var previousState = s_ActiveStateGO.GetComponent<GameStateBehaviour>();

                if (previousState.Persists && previousState.ActiveState == ActiveState)
                {
                    //we need to make way for the DontDestroyOnLoad state that already exists.
                    Destroy(gameObject);
                    return;
                }

                //otherwise, the old state is going away. Either it wasn't a Persisting state, or it was,
                //but we're a different kind of state. In either case, we're going to be replacing it.
                Destroy(s_ActiveStateGO);
            }

            s_ActiveStateGO = gameObject;
            if (Persists)
            {
                DontDestroyOnLoad(gameObject);
            }
        }

        protected override void OnDestroy()
        {
            if (!Persists)
            {
                s_ActiveStateGO = null;
            }
        }
    }
}