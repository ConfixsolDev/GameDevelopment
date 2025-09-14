using Microsoft.AspNetCore.SignalR;

namespace WargameBoard.Web.Hubs
{
    public class GameHub : Hub
    {
        public async Task JoinGame(string gameId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
        }

        public async Task LeaveGame(string gameId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameId);
        }

        public async Task SendGameUpdate(string gameId, string message)
        {
            await Clients.Group(gameId).SendAsync("ReceiveGameUpdate", message);
        }

        public async Task MoveUnit(string gameId, string unitId, int x, int y)
        {
            await Clients.Group(gameId).SendAsync("UnitMoved", unitId, x, y);
        }

        public async Task UpdateToken(string gameId, string tokenId, object tokenData)
        {
            await Clients.Group(gameId).SendAsync("TokenUpdated", tokenId, tokenData);
        }
    }
}
