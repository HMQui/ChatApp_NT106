using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace ChatApp.Server.Hubs
{
    public class VideoCallHub : Hub
    {
        public async Task SendVideo(string toEmail, string fromEmail, byte[] frameData)
        {
            await Clients.Group(toEmail)
                .SendAsync("ReceiveVideo", fromEmail, frameData);
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        public async Task Register(string myEmail)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, myEmail);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            await base.OnDisconnectedAsync(exception);
        }
    }

}
