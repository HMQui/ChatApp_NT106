using Microsoft.AspNetCore.SignalR;
using ChatApp.Common.DAO;

namespace ChatApp.Server.Hubs
{
    public class StatusAccountHub : Hub
    {
        private static readonly Dictionary<string, string> UserConnections = new();
        private static readonly Dictionary<string, string> UserStatuses = new();
        private static readonly Dictionary<string, List<string>> UserFriends = new();

        public async Task SetOnline(string email)
        {
            UserConnections[email] = Context.ConnectionId;
            UserStatuses[email] = "online";

            var friends = GetUserFriends(email);

            foreach (var friend in friends)
            {
                if (UserConnections.TryGetValue(friend, out var friendConnectionId))
                {
                    await Clients.Client(friendConnectionId).SendAsync("FriendStatusChanged", email, "online");
                }
            }
        }

        public async Task SetOffline(string email)
        {
            UserStatuses[email] = "offline";

            var friends = GetUserFriends(email);

            foreach (var friend in friends)
            {
                if (UserConnections.TryGetValue(friend, out var friendConnectionId))
                {
                    await Clients.Client(friendConnectionId).SendAsync("FriendStatusChanged", email, "offline");
                }
            }
        }

        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            var user = UserConnections.FirstOrDefault(x => x.Value == Context.ConnectionId);
            if (user.Key != null)
            {
                UserConnections.Remove(user.Key);
                await SetOffline(user.Key);
            }

            await base.OnDisconnectedAsync(exception);
        }

        private List<string> GetUserFriends(string email)
        {
            try
            {
                var listFriend = FriendDAO.Instance.GetFriends(email);
                var friendEmails = listFriend.Select(friend => friend.Email).ToList();


                return friendEmails;
            }
            catch (Exception ex)
            {
                return new List<string>();
            }
        }
    }

}
