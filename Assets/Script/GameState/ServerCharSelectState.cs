using System;
using System.Collections;
using Script.ConnectionManagement;
using Script.Game.GameplayObject;
using Script.Utils;
using Unity.Multiplayer.Samples.BossRoom;
using Unity.Multiplayer.Samples.Utilities;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using NetworkSceneManager = Script.Networks.NetworkSceneManager;

namespace Script.GameState
{
    [RequireComponent(typeof(NetcodeHooks), typeof(NetworkCharSelection))]
    public class ServerCharSelectState : GameStateBehaviour
    {
        [SerializeField] private NetcodeHooks netcodeHooks;
        protected override GameState ActiveState => GameState.CharSelect;

        public NetworkCharSelection networkCharSelection { get; private set; }
        
        private Coroutine _waitToEndLobbyCoroutine;

        [Inject] private ConnectionManager _connectionManager;
        
        protected override void Awake()
        {
            base.Awake();
            networkCharSelection = GetComponent<NetworkCharSelection>();
            
            netcodeHooks.OnNetworkSpawnHook += OnNetworkSpawn;
            netcodeHooks.OnNetworkDespawnHook += OnNetworkDespawn;
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (netcodeHooks)
            {
                netcodeHooks.OnNetworkSpawnHook -= OnNetworkSpawn;
                netcodeHooks.OnNetworkDespawnHook -= OnNetworkDespawn;
            }
        }

        private void OnClientChangedSeat(ulong clientId, int newSeatIdx, bool lockedIn)
        {
            Debug.Log($"OnClientChangedSeat: client ID {clientId} changed seat to {newSeatIdx} (lockedIn={lockedIn})");
            int idx = FindLobbyPlayerIdx(clientId);
            if (idx == -1)
            {
                throw new Exception($"OnClientChangedSeat: client ID {clientId} is not a lobby player and cannot change seats! Shouldn't be here!");
            }

            if (networkCharSelection.IsLobbyClosed.Value)
            {
                // The user tried to change their class after everything was locked in... too late! Discard this choice
                return;
            }

            if (newSeatIdx == -1)
            {
                // we can't lock in if we're not in a seat
                lockedIn = false;
            }
            else
            {
                foreach (NetworkCharSelection.LobbyPlayerState playerInfo in networkCharSelection.LobbyPlayerStates)
                {
                    if (playerInfo.ClientId != clientId && playerInfo.SeatIdx == newSeatIdx && playerInfo.SeatState == NetworkCharSelection.SeatState.LockedIn)
                    {
                        // someone else is already locked in to this seat, so we can't lock in
                        // Instead of granting lock request, change this player to Inactive state
                        networkCharSelection.LobbyPlayerStates[idx] = new NetworkCharSelection.LobbyPlayerState(
                            clientId,
                            networkCharSelection.LobbyPlayerStates[idx].PlayerName,
                            networkCharSelection.LobbyPlayerStates[idx].PlayerNumber,
                            NetworkCharSelection.SeatState.Inactive);
                        return;
                    }
                }
            }
            
            networkCharSelection.LobbyPlayerStates[idx] = new NetworkCharSelection.LobbyPlayerState(
                clientId,
                networkCharSelection.LobbyPlayerStates[idx].PlayerName,
                networkCharSelection.LobbyPlayerStates[idx].PlayerNumber,
                lockedIn ? NetworkCharSelection.SeatState.LockedIn : NetworkCharSelection.SeatState.Active,
                newSeatIdx,
                Time.time);

            if (lockedIn)
            {
                // to help the clients visually keep track of who's in what seat, we'll "kick out" any other players
                // who were also in that seat. (Those players didn't click "Ready!" fast enough, somebody else took their seat!)
                for (int i = 0; i < networkCharSelection.LobbyPlayerStates.Count; i++)
                {
                    if (networkCharSelection.LobbyPlayerStates[i].SeatIdx == newSeatIdx && i != idx)
                    {
                        // this player was in the same seat as the player who just locked in, so kick them out
                        networkCharSelection.LobbyPlayerStates[i] = new NetworkCharSelection.LobbyPlayerState(
                            networkCharSelection.LobbyPlayerStates[i].ClientId,
                            networkCharSelection.LobbyPlayerStates[i].PlayerName,
                            networkCharSelection.LobbyPlayerStates[i].PlayerNumber,
                            NetworkCharSelection.SeatState.Inactive);
                    }
                }
            }
            
            CloseLobbyIfReady();
        }
        
        private int FindLobbyPlayerIdx(ulong clientId)
        {
            for (int i = 0; i < networkCharSelection.LobbyPlayerStates.Count; i++)
            {
                if (networkCharSelection.LobbyPlayerStates[i].ClientId == clientId)
                {
                    return i;
                }
            }

            return -1;
        }

        private void CloseLobbyIfReady()
        {
            foreach (NetworkCharSelection.LobbyPlayerState playerInfo in networkCharSelection.LobbyPlayerStates)
            {
                if (playerInfo.SeatState != NetworkCharSelection.SeatState.LockedIn)
                {
                    // not everyone is locked in yet
                    return;
                }
            }
            
            networkCharSelection.IsLobbyClosed.Value = true;

            SaveLobbyResults();
            
            // Delay the transition to the next state so that the clients have time to see the final lobby state
            _waitToEndLobbyCoroutine = StartCoroutine(WaitToEndLobby());
        }

        private void CancelCloseLobby()
        {
            if (_waitToEndLobbyCoroutine != null)
            {
                StopCoroutine(_waitToEndLobbyCoroutine);
            }
            networkCharSelection.IsLobbyClosed.Value = false;
        }

        private void SaveLobbyResults()
        {
            foreach (NetworkCharSelection.LobbyPlayerState playerInfo in networkCharSelection.LobbyPlayerStates)
            {
                NetworkObject player = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(playerInfo.ClientId);

                if (player is not null && player.TryGetComponent(out PersistentPlayer persistentPlayer))
                {
                    // pass avatar GUID to PersistentPlayer
                    // it'd be great to simplify this with something like a NetworkScriptableObjects :(

                    persistentPlayer.NetworkAvatarGuidState.avatarNetworkGuid.Value =
                        networkCharSelection.AvatarConfiguration[playerInfo.SeatIdx].Guid.ToNetworkGuid();
                }
            }
        }

        private IEnumerator WaitToEndLobby()
        {
            yield return new WaitForSeconds(3);
            NetworkSceneManager.Instance.LoadScene("InGame", useNetworkSceneManager: true);
        }

        private void OnNetworkDespawn()
        {
            if (NetworkManager.Singleton)
            {
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
                NetworkManager.Singleton.SceneManager.OnSceneEvent -= OnSceneEvent;
            }

            if (networkCharSelection)
            {
                networkCharSelection.OnClientChangedSeat -= OnClientChangedSeat;
            }
        }
        
        private void OnNetworkSpawn()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                enabled = false;
            }
            else
            {
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;
                networkCharSelection.OnClientChangedSeat += OnClientChangedSeat;
                
                NetworkManager.Singleton.SceneManager.OnSceneEvent += OnSceneEvent;
            }
        }

        private void OnSceneEvent(SceneEvent sceneEvent)
        {
            // We need to filter out the event that are not a client has finished loading the scene
            if (sceneEvent.SceneEventType != SceneEventType.LoadComplete) return;
            // When the client finishes loading the Lobby Map, we will need to Seat it
            SeatNewPlayer(sceneEvent.ClientId);
        }

        private int GetAvailablePlayerNumber()
        {
            for (int possiblePlayerNumber = 0; possiblePlayerNumber < _connectionManager.MaxConnectedPlayers; possiblePlayerNumber++)
            {
                if (IsPlayerNumberAvailable(possiblePlayerNumber))
                {
                    return possiblePlayerNumber;
                }
            }

            return -1;
        }

        private bool IsPlayerNumberAvailable(int playerNumber)
        {
            bool found = false;
            foreach (NetworkCharSelection.LobbyPlayerState playerState in networkCharSelection.LobbyPlayerStates)
            {
                if (playerState.PlayerNumber == playerNumber)
                {
                    found = true;
                    break;
                }
            }
            
            return !found;
        }

        private void SeatNewPlayer(ulong clientId)
        {
            // If lobby is closing and waiting to start the game, cancel to allow that new player to select a character
            if (networkCharSelection.IsLobbyClosed.Value)
            {
                CancelCloseLobby();
            }

            var sessionPlayerData = SessionManager<SessionPlayerData>.Instance.GetPlayerData(clientId);
            if (sessionPlayerData.HasValue)
            {
                SessionPlayerData playerData = sessionPlayerData.Value;
                if (playerData.PlayerNumber == -1 || !IsPlayerNumberAvailable(playerData.PlayerNumber))
                {
                    // If no player num already assigned or if player num is no longer available, get an available one.
                    playerData.PlayerNumber = GetAvailablePlayerNumber();
                }

                if (playerData.PlayerNumber == -1)
                {
                    // Sanity check. We ran out of seats... there was no room!
                    throw new Exception($"we shouldn't be here, connection approval should have refused this connection already for client ID {clientId} and player num {playerData.PlayerNumber}");
                }
                
                networkCharSelection.LobbyPlayerStates.Add(new NetworkCharSelection.LobbyPlayerState(
                    clientId,
                    playerData.PlayerName,
                    playerData.PlayerNumber,
                    NetworkCharSelection.SeatState.Inactive));
                
                SessionManager<SessionPlayerData>.Instance.SetPlayerData(clientId, playerData);
            }
        }
        
        private void OnClientDisconnectCallback(ulong clientId)
        {
            // clear this client's PlayerNumber and any associated visuals (so other players know they're gone).
            for (int i = 0; i < networkCharSelection.LobbyPlayerStates.Count; ++i)
            {
                if (networkCharSelection.LobbyPlayerStates[i].ClientId == clientId)
                {
                    networkCharSelection.LobbyPlayerStates.RemoveAt(i);
                    break;
                }
            }

            if (!networkCharSelection.IsLobbyClosed.Value)
            {
                // If the lobby is not already closing, close if the remaining players are all ready
                CloseLobbyIfReady();
            }
        }
    }
}