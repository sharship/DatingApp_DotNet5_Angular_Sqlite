using System.Diagnostics.Contracts;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace API.SignalR
{
    public class PresenceTracker
    {
        private static readonly Dictionary<string, HashSet<string>> OnlineUsers = new Dictionary<string, HashSet<string>>();

        public Task UserConnected(string username, string connectionId)
        {
            lock (OnlineUsers)
            {
                if (OnlineUsers.ContainsKey(username))
                {
                    OnlineUsers[username].Add(connectionId);
                }
                else
                {
                    OnlineUsers.Add(username, new HashSet<string> { connectionId });
                }
            }


            return Task.CompletedTask;
        }

        public Task UserDisconnected(string username, string connectionId)
        {
            lock (OnlineUsers)
            {
                if (!OnlineUsers.ContainsKey(username))
                {
                    return Task.CompletedTask;
                }

                OnlineUsers[username].Remove(connectionId);

                if (OnlineUsers[username].Count == 0)
                {
                    OnlineUsers.Remove(username);
                }
            }

            return Task.CompletedTask;
        }

        public Task<string[]> GetOnlineUsers()
        {
            string[] onlineUsers;
            lock (OnlineUsers)
            {
                onlineUsers = OnlineUsers.OrderBy(pair => pair.Key).Select(pair => pair.Key).ToArray();
            }

            return Task.FromResult(onlineUsers);
        }


    }
}