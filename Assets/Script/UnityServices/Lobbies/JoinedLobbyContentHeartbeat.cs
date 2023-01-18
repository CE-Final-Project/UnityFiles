using Survival.Game.Infrastructure;
using VContainer;

namespace Survival.Game.UnityServices.Lobbies
{
    public class JoinedLobbyContentHeartbeat
    {
        [Inject] LocalLobby m_LocalLobby;
        [Inject] LocalLobbyUser m_LocalUser;
        [Inject] UpdateRunner m_UpdateRunner;
        [Inject] LobbyServiceFacade m_LobbyServiceFacade;

        private int m_AwaitingQueryCount = 0;
        private bool m_ShouldPushData = false;

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
            if (string.IsNullOrEmpty(lobby.LobbyID))
            {
                EndTracking();
            }

            m_ShouldPushData = true;
        }

        /// <summary>
        /// If there have been any data changes since the last update, push them to Lobby.
        /// (Unless we're already awaiting a query, in which case continue waiting.)
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
                    m_AwaitingQueryCount++; // TODO: this should disappear once we use await correctly. This causes issues at the moment if OnSuccess isn't called properly
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