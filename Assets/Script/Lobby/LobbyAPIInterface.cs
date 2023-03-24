using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

namespace Script.Lobby
{
    /// <summary>
    /// Wrapper for all the interactions with the Lobby API.
    /// </summary>
    public class LobbyAPIInterface
    {
        private const int MaxLobbiesToShow = 16; // If more are necessary, consider retrieving paginated results or using filters.

        private readonly List<QueryFilter> _filters;
        private readonly List<QueryOrder> _order;

        public LobbyAPIInterface()
        {
            // Filter for open lobbies only
            _filters = new List<QueryFilter>()
            {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0")
            };

            // Order by newest lobbies first
            _order = new List<QueryOrder>()
            {
                new QueryOrder(
                    asc: false,
                    field: QueryOrder.FieldOptions.Created)
            };
        }

        public async Task<Unity.Services.Lobbies.Models.Lobby> CreateLobby(string requesterUasId, string lobbyName, int maxPlayers, bool isPrivate, Dictionary<string, PlayerDataObject> hostUserData, Dictionary<string, DataObject> lobbyData)
        {
            CreateLobbyOptions createOptions = new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
                Player = new Player(id: requesterUasId, data: hostUserData),
                Data = lobbyData
            };

            return await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createOptions);
        }

        public static async Task DeleteLobby(string lobbyId)
        {
            await LobbyService.Instance.DeleteLobbyAsync(lobbyId);
        }

        public async Task<Unity.Services.Lobbies.Models.Lobby> JoinLobbyByCode(string requesterUasId, string lobbyCode, Dictionary<string, PlayerDataObject> localUserData)
        {
            JoinLobbyByCodeOptions joinOptions = new JoinLobbyByCodeOptions { Player = new Player(id: requesterUasId, data: localUserData) };
            return await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinOptions);
        }

        public async Task<Unity.Services.Lobbies.Models.Lobby> JoinLobbyById(string requesterUasId, string lobbyId, Dictionary<string, PlayerDataObject> localUserData)
        {
            JoinLobbyByIdOptions joinOptions = new JoinLobbyByIdOptions { Player = new Player(id: requesterUasId, data: localUserData) };
            return await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, joinOptions);
        }

        public async Task<Unity.Services.Lobbies.Models.Lobby> QuickJoinLobby(string requesterUasId, Dictionary<string, PlayerDataObject> localUserData)
        {
            QuickJoinLobbyOptions joinRequest = new QuickJoinLobbyOptions
            {
                Filter = _filters,
                Player = new Player(id: requesterUasId, data: localUserData)
            };

            return await LobbyService.Instance.QuickJoinLobbyAsync(joinRequest);
        }

        public async Task<Unity.Services.Lobbies.Models.Lobby> ReconnectToLobby(string lobbyId)
        {
            return await LobbyService.Instance.ReconnectToLobbyAsync(lobbyId);
        }

        public static async Task RemovePlayerFromLobby(string requesterUasId, string lobbyId)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(lobbyId, requesterUasId);
            }
            catch (LobbyServiceException e)
                when (e is { Reason: LobbyExceptionReason.PlayerNotFound })
            {
                // If Player is not found, they have already left the lobby or have been kicked out. No need to throw here
            }
        }

        public async Task<QueryResponse> QueryAllLobbies()
        {
            QueryLobbiesOptions queryOptions = new QueryLobbiesOptions
            {
                Count = MaxLobbiesToShow,
                Filters = _filters,
                Order = _order
            };

            return await LobbyService.Instance.QueryLobbiesAsync(queryOptions);
        }

        public async Task<Unity.Services.Lobbies.Models.Lobby> GetLobby(string lobbyId)
        {
            return await LobbyService.Instance.GetLobbyAsync(lobbyId);
        }

        public async Task<Unity.Services.Lobbies.Models.Lobby> UpdateLobby(string lobbyId, Dictionary<string, DataObject> data, bool shouldLock)
        {
            UpdateLobbyOptions updateOptions = new UpdateLobbyOptions { Data = data, IsLocked = shouldLock };
            return await LobbyService.Instance.UpdateLobbyAsync(lobbyId, updateOptions);
        }

        public async Task<Unity.Services.Lobbies.Models.Lobby> UpdatePlayer(string lobbyId, string playerId, Dictionary<string, PlayerDataObject> data, string allocationId, string connectionInfo)
        {
            UpdatePlayerOptions updateOptions = new UpdatePlayerOptions
            {
                Data = data,
                AllocationId = allocationId,
                ConnectionInfo = connectionInfo
            };
            return await LobbyService.Instance.UpdatePlayerAsync(lobbyId, playerId, updateOptions);
        }

        public static async void SendHeartbeatPing(string lobbyId)
        {
            await LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
        }
    }
}
