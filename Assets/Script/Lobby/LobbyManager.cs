using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Script.Game;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace Script.Lobby
{
    public class LobbyManager : IDisposable
    {
        private const string RelayCodeKey = nameof(LocalLobby.RelayCode);
        private const string LobbyStateKey = nameof(LocalLobby.LocalLobbyState);
        
        private const string DisplayNameKey = nameof(LocalPlayer.DisplayName);
        private const string UserStatusKey = nameof(LocalPlayer.UserStatus);
        private const string CharacterKey = nameof(LocalPlayer.Character);

        private Unity.Services.Lobbies.Models.Lobby _currentLobby;
        private Unity.Services.Lobbies.Models.Lobby CurrentLobby => _currentLobby;
        private LobbyEventCallbacks _lobbyEventCallbacks = new LobbyEventCallbacks();
        private const int MaxLobbyToShow = 10;
        
        private Task _heatBeatTask;
        
        #region Rate Limiting

        public enum RequestType
        {
            Query = 0,
            Join,
            QuickJoin,
            Host
        }

        public bool InLobby()
        {
            if (_currentLobby == null)
            {
                Debug.LogWarning("LobbyManager not currently in a lobby. Did you CreateLobbyAsync or JoinLobbyAsync?");
                return false;
            }

            return true;
        }

        public ServiceRateLimiter GetRateLimit(RequestType type)
        {
            return type switch
            {
                RequestType.Join => _joinCooldown,
                RequestType.QuickJoin => _quickJoinCooldown,
                RequestType.Host => _createCooldown,
                _ => _queryCooldown
            };
        }

        // Rate Limits are posted here: https://docs.unity.com/lobby/rate-limits.html

        private readonly ServiceRateLimiter _queryCooldown = new ServiceRateLimiter(1, 1f);
        private readonly ServiceRateLimiter _createCooldown = new ServiceRateLimiter(2, 6f);
        private readonly ServiceRateLimiter _joinCooldown = new ServiceRateLimiter(2, 6f);
        private readonly ServiceRateLimiter _quickJoinCooldown = new ServiceRateLimiter(1, 10f);
        private ServiceRateLimiter _getLobbyCooldown = new ServiceRateLimiter(1, 1f);
        private ServiceRateLimiter _deleteLobbyCooldown = new ServiceRateLimiter(2, 1f);
        private ServiceRateLimiter _updateLobbyCooldown = new ServiceRateLimiter(5, 5f);
        private ServiceRateLimiter _updatePlayerCooldown = new ServiceRateLimiter(5, 5f);
        private ServiceRateLimiter _leaveLobbyOrRemovePlayer = new ServiceRateLimiter(5, 1);
        private readonly ServiceRateLimiter _heartBeatCooldown = new ServiceRateLimiter(5, 30);

        #endregion

        private static Dictionary<string, PlayerDataObject> CreateInitialPlayerData(LocalPlayer user)
        {
            var data = new Dictionary<string, PlayerDataObject>();
            
            PlayerDataObject displayNameObject = new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, user.DisplayName.Value);
            data.Add("DisplayName", displayNameObject);
            return data;
        }
        
        public async Task<Unity.Services.Lobbies.Models.Lobby> CreateLobbyAsync(string lobbyName, int maxPlayers, bool isPrivate, LocalPlayer localPlayer)
        {
            if (_createCooldown.IsCoolingDown)
            {
                Debug.LogWarning("CreateLobbyAsync is on cooldown. Please wait.");
                return null;
            }

            await _createCooldown.QueueUntilCooldown();

            try
            {
                string userId = AuthenticationService.Instance.PlayerId;
                
                CreateLobbyOptions createOptions = new CreateLobbyOptions
                {
                    IsPrivate = isPrivate,
                    Player = new Player(id: userId, data: CreateInitialPlayerData(localPlayer))
                };
                _currentLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createOptions);

                return _currentLobby;
            }
            catch (Exception ex)
            {
                Debug.LogError($"CreateLobbyAsync failed: {ex.Message}");
                return null;
            }
        }

        public async Task<Unity.Services.Lobbies.Models.Lobby> JoinLobbyAsync(string lobbyId, string lobbyCode,
            LocalPlayer localUser)
        {
            if (_joinCooldown.IsCoolingDown || (lobbyId== null && lobbyCode == null))
            {
                return null;
            }

            await _joinCooldown.QueueUntilCooldown();
            
            string userId = AuthenticationService.Instance.PlayerId;
            var playerData = CreateInitialPlayerData(localUser);

            if (!string.IsNullOrEmpty(lobbyId))
            {
                JoinLobbyByIdOptions joinOptions = new JoinLobbyByIdOptions
                {
                    Player = new Player(id: userId, data: playerData)
                };
                _currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, joinOptions);
            }
            else
            {
                JoinLobbyByCodeOptions joinOptions = new JoinLobbyByCodeOptions
                {
                    Player = new Player(id: userId, data: playerData)
                };
                _currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinOptions);
            }
            
            return _currentLobby;
        }
        
        public async Task<Unity.Services.Lobbies.Models.Lobby> QuickJoinLobbyAsync(LocalPlayer localUser)
        {
            if (_quickJoinCooldown.IsCoolingDown)
            {
                return null;
            }

            await _quickJoinCooldown.QueueUntilCooldown();
            
            string userId = AuthenticationService.Instance.PlayerId;
            var playerData = CreateInitialPlayerData(localUser);

            QuickJoinLobbyOptions joinOptions = new QuickJoinLobbyOptions
            {
                Player = new Player(id: userId, data: playerData)
            };
            _currentLobby = await LobbyService.Instance.QuickJoinLobbyAsync(joinOptions);
            
            return _currentLobby;
        }
        
        public async Task<QueryResponse> RetrieveLobbyListAsync()
        {
            if (_queryCooldown.TaskQueued)
            {
                return null;
            }

            await _queryCooldown.QueueUntilCooldown();
            
            QueryLobbiesOptions queryOptions = new QueryLobbiesOptions
            {
                Count = MaxLobbyToShow
            };
            
            return await LobbyService.Instance.QueryLobbiesAsync(queryOptions);
        }

        public async Task BindLocalLobbyToRemote(string lobbyId, LocalLobby localLobby)
        {
            _lobbyEventCallbacks.LobbyChanged += async changes =>
            {
                if (changes.LobbyDeleted)
                {
                    await LeaveLobbyAsync();
                    return;
                }

                //Lobby Fields
                if (changes.Name.Changed)
                    localLobby.LobbyName.Value = changes.Name.Value;
                if (changes.HostId.Changed)
                    localLobby.HostID.Value = changes.HostId.Value;
                if (changes.IsPrivate.Changed)
                    localLobby.Private.Value = changes.IsPrivate.Value;
                if (changes.IsLocked.Changed)
                    localLobby.Locked.Value = changes.IsLocked.Value;
                if (changes.AvailableSlots.Changed)
                    localLobby.AvailableSlots.Value = changes.AvailableSlots.Value;
                if (changes.MaxPlayers.Changed)
                    localLobby.MaxPlayerCount.Value = changes.MaxPlayers.Value;

                if (changes.LastUpdated.Changed)
                    localLobby.LastUpdated.Value = changes.LastUpdated.Value.ToFileTimeUtc();

                //Custom Lobby Fields
                if (changes.Data.Changed)
                    LobbyChanged();

                if (changes.PlayerJoined.Changed)
                    PlayersJoined();

                if (changes.PlayerLeft.Changed)
                    PlayersLeft();

                if (changes.PlayerData.Changed)
                    PlayerDataChanged();

                void LobbyChanged()
                {
                    foreach ((string changedKey, var changedValue) in changes.Data.Value)
                    {
                        if (changedValue.Removed)
                        {
                            RemoveCustomLobbyData(changedKey);
                        }

                        if (changedValue.Changed)
                        {
                            ParseCustomLobbyData(changedKey, changedValue.Value);
                        }
                    }

                    void RemoveCustomLobbyData(string changedKey)
                    {
                        if (changedKey == RelayCodeKey)
                            localLobby.RelayCode.Value = "";
                    }

                    void ParseCustomLobbyData(string changedKey, DataObject playerDataObject)
                    {
                        switch (changedKey)
                        {
                            case RelayCodeKey:
                                localLobby.RelayCode.Value = playerDataObject.Value;
                                break;
                            case LobbyStateKey:
                                localLobby.LocalLobbyState.Value = Enum.Parse<LobbyState>(playerDataObject.Value);
                                break;
                        }
                    }
                }

                void PlayersJoined()
                {
                    foreach (LobbyPlayerJoined playerChanges in changes.PlayerJoined.Value)
                    {
                        Player joinedPlayer = playerChanges.Player;

                        string id = joinedPlayer.Id;
                        int index = playerChanges.PlayerIndex;
                        bool isHost = localLobby.HostID.Value == id;

                        LocalPlayer newPlayer = new LocalPlayer(id, index, isHost);

                        foreach ((string key, PlayerDataObject dataObject) in joinedPlayer.Data)
                        {
                            ParseCustomPlayerData(newPlayer, key, dataObject.Value);
                        }

                        localLobby.AddPlayer(index, newPlayer);
                    }
                }

                void PlayersLeft()
                {
                    foreach (int leftPlayerIndex in changes.PlayerLeft.Value)
                    {
                        localLobby.RemovePlayer(leftPlayerIndex);
                    }
                }

                void PlayerDataChanged()
                {
                    foreach ((int playerIndex, LobbyPlayerChanges playerChanges) in changes.PlayerData.Value)
                    {
                        LocalPlayer localPlayer = localLobby.GetLocalPlayer(playerIndex);
                        if (localPlayer == null)
                            continue;
                        if (playerChanges.ConnectionInfoChanged.Changed)
                        {
                            string connectionInfo = playerChanges.ConnectionInfoChanged.Value;
                            Debug.Log(
                                $"ConnectionInfo for player {playerIndex} changed to {connectionInfo}");
                        }

                        if (playerChanges.LastUpdatedChanged.Changed)
                        {
                        }

                        //There are changes on the Player
                        if (playerChanges.ChangedData.Changed)
                        {
                            foreach ((string key, var changedValue) in playerChanges.ChangedData.Value)
                            {
                                //There are changes on some of the changes in the player list of changes

                                if (changedValue.Changed)
                                {
                                    if (changedValue.Removed)
                                    {
                                        Debug.LogWarning("This Sample does not remove Player Values currently.");
                                        continue;
                                    }

                                    PlayerDataObject playerDataObject = changedValue.Value;
                                    ParseCustomPlayerData(localPlayer, key, playerDataObject.Value);
                                }
                            }
                        }
                    }
                }
            };
            
            _lobbyEventCallbacks.LobbyEventConnectionStateChanged += lobbyEventConnectionState =>
            {
                Debug.Log($"Lobby ConnectionState Changed to {lobbyEventConnectionState}");
            };

            _lobbyEventCallbacks.KickedFromLobby += () =>
            {
                Debug.Log("Left Lobby");
                Dispose();
            };
            await LobbyService.Instance.SubscribeToLobbyEventsAsync(lobbyId, _lobbyEventCallbacks);
        }

        private static void ParseCustomPlayerData(LocalPlayer player, string dataKey, string playerDataValue)
        {
            switch (dataKey)
            {
                case CharacterKey:
                    player.Character.Value = Enum.Parse<CharacterType>(playerDataValue);
                    break;
                case UserStatusKey:
                    player.UserStatus.Value = Enum.Parse<PlayerStatus>(playerDataValue);
                    break;
                case DisplayNameKey:
                    player.DisplayName.Value = playerDataValue;
                    break;
            }
        }

        public async Task<Unity.Services.Lobbies.Models.Lobby> GetLobbyAsync(string lobbyId = null)
        {
            if (!InLobby())
                return null;
            await _createCooldown.QueueUntilCooldown();
            lobbyId ??= _currentLobby.Id;
            return _currentLobby = await LobbyService.Instance.GetLobbyAsync(lobbyId);
        }

        public async Task LeaveLobbyAsync()
        {
            await _leaveLobbyOrRemovePlayer.QueueUntilCooldown();
            if (!InLobby())
                return;
            string playerId = AuthenticationService.Instance.PlayerId;

            await LobbyService.Instance.RemovePlayerAsync(_currentLobby.Id, playerId);
            Dispose();
        }

        public async Task UpdatePlayerDataAsync(Dictionary<string, string> data)
        {
            if (!InLobby())
                return;

            string playerId = AuthenticationService.Instance.PlayerId;
            var dataCurr = new Dictionary<string, PlayerDataObject>();
            foreach (var dataNew in data)
            {
                PlayerDataObject dataObj = new PlayerDataObject(visibility: PlayerDataObject.VisibilityOptions.Member,
                    value: dataNew.Value);
                if (dataCurr.ContainsKey(dataNew.Key))
                    dataCurr[dataNew.Key] = dataObj;
                else
                    dataCurr.Add(dataNew.Key, dataObj);
            }

            if (_updatePlayerCooldown.TaskQueued)
                return;
            await _updatePlayerCooldown.QueueUntilCooldown();

            UpdatePlayerOptions updateOptions = new UpdatePlayerOptions
            {
                Data = dataCurr,
                AllocationId = null,
                ConnectionInfo = null
            };
            _currentLobby = await LobbyService.Instance.UpdatePlayerAsync(_currentLobby.Id, playerId, updateOptions);
        }

        public async Task UpdatePlayerRelayInfoAsync(string lobbyID, string allocationId, string connectionInfo)
        {
            if (!InLobby())
                return;

            string playerId = AuthenticationService.Instance.PlayerId;

            if (_updatePlayerCooldown.TaskQueued)
                return;
            await _updatePlayerCooldown.QueueUntilCooldown();

            UpdatePlayerOptions updateOptions = new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject>(),
                AllocationId = allocationId,
                ConnectionInfo = connectionInfo
            };
            _currentLobby = await LobbyService.Instance.UpdatePlayerAsync(lobbyID, playerId, updateOptions);
        }

        public async Task UpdateLobbyDataAsync(Dictionary<string, string> data)
        {
            if (!InLobby())
                return;

            var dataCurr = _currentLobby.Data ?? new Dictionary<string, DataObject>();

            bool shouldLock = false;
            foreach (var dataNew in data)
            {
                // Special case: We want to be able to filter on our color data, so we need to supply an arbitrary index to retrieve later. Uses N# for numerics, instead of S# for strings.
                DataObject.IndexOptions index = dataNew.Key == "LocalLobbyColor" ? DataObject.IndexOptions.N1 : 0;
                DataObject dataObj = new DataObject(DataObject.VisibilityOptions.Public, dataNew.Value,
                    index); // Public so that when we request the list of lobbies, we can get info about them for filtering.
                if (dataCurr.ContainsKey(dataNew.Key))
                    dataCurr[dataNew.Key] = dataObj;
                else
                    dataCurr.Add(dataNew.Key, dataObj);

                //Special Use: Get the state of the Local lobby so we can lock it from appearing in queries if it's not in the "Lobby" LocalLobbyState
                if (dataNew.Key == "LocalLobbyState")
                {
                    Enum.TryParse(dataNew.Value, out LobbyState lobbyState);
                    shouldLock = lobbyState != LobbyState.Lobby;
                }
            }

            //We can still update the latest data to send to the service, but we will not send multiple UpdateLobbySyncCalls
            if (_updateLobbyCooldown.TaskQueued)
                return;
            await _updateLobbyCooldown.QueueUntilCooldown();

            UpdateLobbyOptions updateOptions = new UpdateLobbyOptions { Data = dataCurr, IsLocked = shouldLock };
            _currentLobby = await LobbyService.Instance.UpdateLobbyAsync(_currentLobby.Id, updateOptions);
        }

        public async Task DeleteLobbyAsync()
        {
            if (!InLobby())
                return;
            await _deleteLobbyCooldown.QueueUntilCooldown();

            await LobbyService.Instance.DeleteLobbyAsync(_currentLobby.Id);
        }
        
        public void Dispose()
        {
            _currentLobby = null;
        }
        
        #region HeartBeat

//Since the LobbyManager maintains the "connection" to the lobby, we will continue to heartbeat until host leaves.
        private async Task SendHeartbeatPingAsync()
        {
            if (!InLobby())
                return;
            if (_heartBeatCooldown.IsCoolingDown)
                return;
            await _heartBeatCooldown.QueueUntilCooldown();

            await LobbyService.Instance.SendHeartbeatPingAsync(_currentLobby.Id);
        }

        private void StartHeartBeat()
        {
            _heatBeatTask = HeartBeatLoop();
        }

        private async Task HeartBeatLoop()
        {
            while (_currentLobby != null)
            {
                await SendHeartbeatPingAsync();
                await Task.Delay(8000);
            }
        }

        #endregion
    }
    
    //Manages the Amount of times you can hit a service call.
    //Adds a buffer to account for ping times.
    //Will Queue the latest overflow task for when the cooldown ends.
    //Created to mimic the way rate limits are implemented Here:  https://docs.unity.com/lobby/rate-limits.html
    public class ServiceRateLimiter
    {
        private Action<bool> _onCooldownChange;
        private readonly int _coolDownMS;
        internal bool TaskQueued { get; set; } = false;

        private readonly int _serviceCallTimes;
        private bool _coolingDown = false;
        private int _taskCounter;

        //(If you're still getting rate limit errors, try increasing the pingBuffer)
        public ServiceRateLimiter(int callTimes, float coolDown, int pingBuffer = 100)
        {
            _serviceCallTimes = callTimes;
            _taskCounter = _serviceCallTimes;
            _coolDownMS =
                Mathf.CeilToInt(coolDown * 1000) +
                pingBuffer;
        }

        public async Task QueueUntilCooldown()
        {
            if (!_coolingDown)
            {
#pragma warning disable 4014
                ParallelCooldownAsync();
#pragma warning restore 4014
            }

            _taskCounter--;

            if (_taskCounter > 0)
            {
                return;
            }

            if (!TaskQueued)
                TaskQueued = true;
            else
                return;

            while (_coolingDown)
            {
                await Task.Delay(10);
            }
        }

        private async Task ParallelCooldownAsync()
        {
            IsCoolingDown = true;
            await Task.Delay(_coolDownMS);
            IsCoolingDown = false;
            TaskQueued = false;
            _taskCounter = _serviceCallTimes;
        }

        public bool IsCoolingDown
        {
            get => _coolingDown;
            private set
            {
                if (_coolingDown != value)
                {
                    _coolingDown = value;
                    _onCooldownChange?.Invoke(_coolingDown);
                }
            }
        }
    }
}