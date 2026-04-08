using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
namespace WorkExchange.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        //連線事件
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        //離線事件
        public override async Task OnDisconnectedAsync(Exception ex)
        {
            await base.OnDisconnectedAsync(ex);
        }

        //加入房間
        public Task joinRoom(string roomId) => Groups.AddToGroupAsync(Context.ConnectionId, roomId);

      
    }
}
