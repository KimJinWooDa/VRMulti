using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

namespace disguys.Looby
{
     /// <summary>
    /// 이것은 Lobby API와의 모든 상호 작용을 래핑하는 래퍼 클래스입니다.
    /// 이 클래스는 API 호출을 추상화하고 적절한 인자를 전달하는 데 도움이 됩니다.
    /// 이 클래스는 또한 필요한 경우 API 호출에 대한 캐싱 및 결과 처리를 제공합니다.
    /// 이 클래스의 인스턴스는 게임의 라이프사이클 동안 계속해서 사용됩니다.
    /// 이것은 게임에서 여러 개의 API 호출이 필요할 때 이들을 조직화하고 중복을 피하기 위해 유용합니다.
    /// </summary>
    public class LobbyAPIInterface
    {
        const int k_MaxLobbiesToShow = 16; // If more are necessary, consider retrieving paginated results or using filters.

        readonly List<QueryFilter> m_Filters;
        readonly List<QueryOrder> m_Order;

        public LobbyAPIInterface()
        {
            // Filter for open lobbies only
            m_Filters = new List<QueryFilter>()
            {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0")
            };

            // Order by newest lobbies first
            m_Order = new List<QueryOrder>()
            {
                new QueryOrder(
                    asc: false,
                    field: QueryOrder.FieldOptions.Created)
            };
        }

        public async Task<Lobby> CreateLobby(string requesterUasId, string lobbyName, int maxPlayers, bool isPrivate, Dictionary<string, PlayerDataObject> hostUserData, Dictionary<string, DataObject> lobbyData)
        {
            CreateLobbyOptions createOptions = new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
                Player = new Player(id: requesterUasId, data: hostUserData),
                Data = lobbyData
            };

            return await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createOptions);
        }

        public async Task DeleteLobby(string lobbyId)
        {
            await LobbyService.Instance.DeleteLobbyAsync(lobbyId);
        }

        public async Task<Lobby> JoinLobbyByCode(string requesterUasId, string lobbyCode, Dictionary<string, PlayerDataObject> localUserData)
        {
            JoinLobbyByCodeOptions joinOptions = new JoinLobbyByCodeOptions { Player = new Player(id: requesterUasId, data: localUserData) };
            return await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinOptions);
        }

        public async Task<Lobby> JoinLobbyById(string requesterUasId, string lobbyId, Dictionary<string, PlayerDataObject> localUserData)
        {
            JoinLobbyByIdOptions joinOptions = new JoinLobbyByIdOptions { Player = new Player(id: requesterUasId, data: localUserData) };
            return await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, joinOptions);
        }

        public async Task<Lobby> QuickJoinLobby(string requesterUasId, Dictionary<string, PlayerDataObject> localUserData)
        {
            var joinRequest = new QuickJoinLobbyOptions
            {
                Filter = m_Filters,
                Player = new Player(id: requesterUasId, data: localUserData)
            };

            return await LobbyService.Instance.QuickJoinLobbyAsync(joinRequest);
        }

        public async Task<Lobby> ReconnectToLobby(string lobbyId)
        {
            return await LobbyService.Instance.ReconnectToLobbyAsync(lobbyId);
        }

        public async Task RemovePlayerFromLobby(string requesterUasId, string lobbyId)
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
                Count = k_MaxLobbiesToShow,
                Filters = m_Filters,
                Order = m_Order
            };

            return await LobbyService.Instance.QueryLobbiesAsync(queryOptions);
        }

        public async Task<Lobby> GetLobby(string lobbyId)
        {
            return await LobbyService.Instance.GetLobbyAsync(lobbyId);
        }

        public async Task<Lobby> UpdateLobby(string lobbyId, Dictionary<string, DataObject> data, bool shouldLock)
        {
            UpdateLobbyOptions updateOptions = new UpdateLobbyOptions { Data = data, IsLocked = shouldLock };
            return await LobbyService.Instance.UpdateLobbyAsync(lobbyId, updateOptions);
        }

        public async Task<Lobby> UpdatePlayer(string lobbyId, string playerId, Dictionary<string, PlayerDataObject> data, string allocationId, string connectionInfo)
        {
            UpdatePlayerOptions updateOptions = new UpdatePlayerOptions
            {
                Data = data,
                AllocationId = allocationId,
                ConnectionInfo = connectionInfo
            };
            return await LobbyService.Instance.UpdatePlayerAsync(lobbyId, playerId, updateOptions);
        }

        public async void SendHeartbeatPing(string lobbyId)
        {
            await LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
        }
    }
}