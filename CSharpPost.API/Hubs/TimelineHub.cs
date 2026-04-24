using Microsoft.AspNetCore.SignalR;

namespace MicrobloggingApp.API.Hubs
{
    public class TimelineHub : Hub
    {
        // Method for broadcasting a new post
        public async Task BroadcastNewPost(string postText, string imagePath, string createdAt)
        {
            await Clients.All.SendAsync("ReceiveNewPost", postText, imagePath, createdAt);
        }
    }
}
