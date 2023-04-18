using disguys.Infrastructure;
using VContainer;

namespace disguys.Looby
{
    /// <summary>
    /// 이 코드는 로비의 속도 제한에 준수하면서 참여한 로비의 변경 사항을 계속해서 업데이트하는 데 사용됩니다.
    /// 즉, 이 코드는 Lobby API와 상호작용하여 로비 내부의 변경 사항을 모니터링하고,
    /// 로비 상태의 변경 사항을 알리기 위한 이벤트를 발생시킵니다.
    /// 이를 통해 로컬 로비 사용자는 다른 사용자와의 상호작용을 유지하면서 로비 내부의 상태를 업데이트할 수 있습니다.
    /// 이를 위해 Lobby API와의 상호작용에 대한 세부사항을 캡슐화하는 LobbyClient 인스턴스를 사용합니다.
    /// </summary>
    public class JoinedLobbyContentHeartbeat
    {
        [Inject] LocalLobby m_LocalLobby;
        [Inject] LocalLobbyUser m_LocalUser;
        [Inject] UpdateRunner m_UpdateRunner;
        [Inject] LobbyServiceFacade m_LobbyServiceFacade;

        int m_AwaitingQueryCount = 0;
        bool m_ShouldPushData = false;

        public void BeginTracking()
        {
            m_UpdateRunner.Subscribe(OnUpdate, 1.5f);
            m_LocalLobby.changed += OnLocalLobbyChanged;
            m_ShouldPushData = true; // Ensure the initial presence of a new player is pushed to the lobby; otherwise, when a non-host joins, the LocalLobby never receives their data until they push something new.
        }

        public void EndTracking()
        {
            m_ShouldPushData = false;
            m_UpdateRunner.Unsubscribe(OnUpdate);
            m_LocalLobby.changed -= OnLocalLobbyChanged;
        }

        void OnLocalLobbyChanged(LocalLobby lobby)
        {
            if (string.IsNullOrEmpty(lobby.LobbyID)) // When the player leaves, their LocalLobby is cleared out but maintained.
            {
                EndTracking();
            }

            m_ShouldPushData = true;
        }

        /// <summary>
        /// 만약 마지막 업데이트 이후 데이터 변경이 있었다면 이를 로비에 푸시합니다.
        /// (이미 쿼리를 기다리고 있는 경우 제외)
        /// </summary>
        async void OnUpdate(float dt)
        {
            if (m_AwaitingQueryCount > 0)
            {
                return;
            }

            if (m_LocalUser.IsHost)
            {
                m_LobbyServiceFacade.DoLobbyHeartbeat(dt);
            }

            if (m_ShouldPushData)
            {
                m_ShouldPushData = false;

                if (m_LocalUser.IsHost)
                {
                    m_AwaitingQueryCount++; // todo this should disapear once we use await correctly. This causes issues at the moment if OnSuccess isn't called properly
                    await m_LobbyServiceFacade.UpdateLobbyDataAsync(m_LocalLobby.GetDataForUnityServices());
                    m_AwaitingQueryCount--;
                }
                m_AwaitingQueryCount++;
                await m_LobbyServiceFacade.UpdatePlayerDataAsync(m_LocalUser.GetDataForUnityServices());
                m_AwaitingQueryCount--;
            }
        }
    }
}