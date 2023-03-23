using System.Collections;
using System.Threading.Tasks;
using Script.Infrastructure.PubSub;
using UnityEngine;
using VContainer;

namespace Script.ConnectionManagement.ConnectionState
{
    internal class ClientReconnectingState : ClientConnectingState
    {
        [Inject] private IPublisher<ReconnectMessage> _reconnectMessagePublisher;

        private Coroutine _reconnectCoroutine;
        private string _lobbyCode = "";
        private int _numberAttempts;

        private const float TimeBetweenAttempts = 5;
        
        public override void Enter()
        {
            _numberAttempts = 0;
            _lobbyCode = LobbyServiceFacade.CurrentUnityLobby != null ? LobbyServiceFacade.CurrentUnityLobby.LobbyCode : "";
            _reconnectCoroutine = ConnectionManager.StartCoroutine(ReconnectCoroutine());
        }

        public override void Exit()
        {
            if (_reconnectCoroutine != null)
            {
                ConnectionManager.StopCoroutine(_reconnectCoroutine);
                _reconnectCoroutine = null;
            }
            _reconnectMessagePublisher.Publish(new ReconnectMessage(ConnectionManager.NumberOfReconnectAttempts, ConnectionManager.NumberOfReconnectAttempts));
        }
        
         public override void OnClientConnected(ulong _)
        {
            ConnectionManager.ChangeState(ConnectionManager.ClientConnected);
        }

        public override void OnClientDisconnect(ulong _)
        {
            string disconnectReason = ConnectionManager.NetworkManager.DisconnectReason;
            if (_numberAttempts < ConnectionManager.NumberOfReconnectAttempts)
            {
                if (string.IsNullOrEmpty(disconnectReason))
                {
                    _reconnectCoroutine = ConnectionManager.StartCoroutine(ReconnectCoroutine());
                }
                else
                {
                    ConnectStatus connectStatus = JsonUtility.FromJson<ConnectStatus>(disconnectReason);
                    ConnectStatusPublisher.Publish(connectStatus);
                    switch (connectStatus)
                    {
                        case ConnectStatus.UserRequestedDisconnect:
                        case ConnectStatus.HostEndedSession:
                        case ConnectStatus.ServerFull:
                        case ConnectStatus.IncompatibleBuildType:
                            ConnectionManager.ChangeState(ConnectionManager.Offline);
                            break;
                        default:
                            _reconnectCoroutine = ConnectionManager.StartCoroutine(ReconnectCoroutine());
                            break;
                    }
                }
            }
            else
            {
                if (string.IsNullOrEmpty(disconnectReason))
                {
                    ConnectStatusPublisher.Publish(ConnectStatus.GenericDisconnect);
                }
                else
                {
                    ConnectStatus connectStatus = JsonUtility.FromJson<ConnectStatus>(disconnectReason);
                    ConnectStatusPublisher.Publish(connectStatus);
                }

                ConnectionManager.ChangeState(ConnectionManager.Offline);
            }
        }

        IEnumerator ReconnectCoroutine()
        {
            // If not on first attempt, wait some time before trying again, so that if the issue causing the disconnect
            // is temporary, it has time to fix itself before we try again. Here we are using a simple fixed cooldown
            // but we could want to use exponential backoff instead, to wait a longer time between each failed attempt.
            // See https://en.wikipedia.org/wiki/Exponential_backoff
            if (_numberAttempts > 0)
            {
                yield return new WaitForSeconds(TimeBetweenAttempts);
            }

            Debug.Log("Lost connection to host, trying to reconnect...");

            ConnectionManager.NetworkManager.Shutdown();

            yield return new WaitWhile(() => ConnectionManager.NetworkManager.ShutdownInProgress); // wait until NetworkManager completes shutting down
            Debug.Log($"Reconnecting attempt {_numberAttempts + 1}/{ConnectionManager.NumberOfReconnectAttempts}...");
            // m_ReconnectMessagePublisher.Publish(new ReconnectMessage(m_NbAttempts, ConnectionManager.NbReconnectAttempts));
            _numberAttempts++;
            if (!string.IsNullOrEmpty(_lobbyCode)) // Attempting to reconnect to lobby.
            {
                // When using Lobby with Relay, if a user is disconnected from the Relay server, the server will notify
                // the Lobby service and mark the user as disconnected, but will not remove them from the lobby. They
                // then have some time to attempt to reconnect (defined by the "Disconnect removal time" parameter on
                // the dashboard), after which they will be removed from the lobby completely.
                // See https://docs.unity.com/lobby/reconnect-to-lobby.html
                var reconnectingToLobby = LobbyServiceFacade.ReconnectToLobbyAsync(LocalLobby?.LobbyID);
                yield return new WaitUntil(() => reconnectingToLobby.IsCompleted);
                
                // If succeeded, attempt to connect to Relay
                if (!reconnectingToLobby.IsFaulted && reconnectingToLobby.Result != null)
                {
                    // If this fails, the OnClientDisconnect callback will be invoked by Netcode
                    Task connectingToRelay = ConnectClientAsync();
                    yield return new WaitUntil(() => connectingToRelay.IsCompleted);
                }
                else
                {
                    Debug.Log("Failed reconnecting to lobby.");
                    // Calling OnClientDisconnect to mark this attempt as failed and either start a new one or give up
                    // and return to the Offline state
                    OnClientDisconnect(0);
                }
            }
            else // If not using Lobby, simply try to reconnect to the server directly
            {
                // If this fails, the OnClientDisconnect callback will be invoked by Netcode
                Task connectingClient = ConnectClientAsync();
                yield return new WaitUntil(() => connectingClient.IsCompleted);
            }
        }
    }
}