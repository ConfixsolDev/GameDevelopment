using Microsoft.AspNetCore.SignalR;

namespace WargameBoard.Web.Hubs
{
    public class PlacementsHub : Hub
    {
        public async Task JoinSession(string sessionId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"session_{sessionId}");
        }

        public async Task LeaveSession(string sessionId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"session_{sessionId}");
        }

        public async Task BroadcastPlacementUpdate(string sessionId, object placementData)
        {
            await Clients.Group($"session_{sessionId}").SendAsync("PlacementsChanged", placementData);
        }

        public async Task BroadcastTokenAssignment(string sessionId, object assignmentData)
        {
            await Clients.Group($"session_{sessionId}").SendAsync("ReceiveTokenAssignment", assignmentData);
        }

        public async Task BroadcastTurnUpdate(string sessionId, object turnData)
        {
            await Clients.Group($"session_{sessionId}").SendAsync("ReceiveTurnUpdate", turnData);
        }

        public async Task BroadcastSessionUpdate(string sessionId, object sessionData)
        {
            await Clients.Group($"session_{sessionId}").SendAsync("ReceiveSessionUpdate", sessionData);
        }
    }
}
