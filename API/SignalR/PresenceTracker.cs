namespace API.SignalR
{
    public class PresenceTracker
    {
        // a dictionary is not thread-safe, this is not production worthy approach
        private static readonly Dictionary<string, List<string>> OnlineUsers =
            new Dictionary<string, List<string>>();

        // use boolean to specify whether user has genuinely come online (as in they
        // didn't have any other connections) or they have gone offline and we have removed
        // all of their connections from the dictionary
        public Task<bool> UserConnected(string username, string connectionId)
        {
            bool isOnline = false;
            lock (OnlineUsers)
            {
                // if they have already got a connection and we're simply adding the connectionId
                // to the key of 'username', then they haven't genuinely come online they've
                // just added another connection and they were already online before...
                if (OnlineUsers.ContainsKey(username))
                {
                    OnlineUsers[username].Add(connectionId);
                }
                else
                {
                    // ...but if we're adding a new entry then we set the isOnline property = true;
                    OnlineUsers.Add(username, new List<string> { connectionId });
                    isOnline = true;
                }
            }

            return Task.FromResult(isOnline);
        }

        public Task<bool> UserDisconnected(string username, string connectionId)
        {
            bool isOffline = false;

            lock (OnlineUsers)
            {
                if (!OnlineUsers.ContainsKey(username)) return Task.FromResult(isOffline);

                OnlineUsers[username].Remove(connectionId);

                if (OnlineUsers[username].Count == 0)
                {
                    OnlineUsers.Remove(username);
                    isOffline = true;
                }
            }

            return Task.FromResult(isOffline);
        }

        public Task<string[]> GetOnlineUsers()
        {
            string[] onlineUsers;
            lock (OnlineUsers)
            {
                onlineUsers = OnlineUsers.OrderBy(k => k.Key).Select(k => k.Key).ToArray();
            }

            return Task.FromResult(onlineUsers);
        }

        public static Task<List<string>> GetConnectionsForUser(string username)
        {
            List<string> connectionIds;
            // Accessing dictionary so lock it as not thread-safe
            // don't want to encounter problems by two concurrent users accessing at same time
            // scalability problems with this method
            // Better off using db if no access to Redis. Redis optimum option.
            // This method is suitable only for training.
            lock (OnlineUsers)
            {
                connectionIds = OnlineUsers.GetValueOrDefault(username);
            }

            return Task.FromResult(connectionIds);
        }
    }
}
