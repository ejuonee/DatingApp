using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.SignalR
{
    public class PresenceTracker
    {
        private static readonly Dictionary<string, HashSet<string>> Connections = new Dictionary<string, HashSet<string>>();

        private static readonly Dictionary<string,List<string>> OnlineUsers = new Dictionary<string, List<string>>();

        public Task<bool> UserConnected(string username, string connectionId)
        {
            bool isOnline =false;
            lock (Connections)
            {
                HashSet<string> connections;
                if (!Connections.TryGetValue(username, out connections))
                {
                    connections = new HashSet<string>();
                    Connections[username] = connections;
                }
                connections.Add(connectionId);
            }
            lock (OnlineUsers)
            {
                List<string> connections;
                if (!OnlineUsers.TryGetValue(username, out connections))
                {
                    connections = new List<string>();
                    OnlineUsers[username] = connections;
                }
                connections.Add(connectionId);
            }

            lock (OnlineUsers){

                if (OnlineUsers.ContainsKey(username))
                {
                    OnlineUsers[username].Add(connectionId);
                }
                else
                {
                    OnlineUsers.Add(username, new List<string> { connectionId });
                    isOnline= true;
                }
            }
            return Task.FromResult(isOnline);
        }

        public Task<bool> UserDisconnected (string username, string connectionId){


            bool isOffline = false;
            lock(OnlineUsers){
                if (!OnlineUsers.ContainsKey(username))
                {
                   return Task.FromResult(isOffline);
                }
                OnlineUsers[username].Remove(connectionId);

                if(OnlineUsers[username].Count == 0)
                {
                    OnlineUsers.Remove(username);
                    isOffline = true;
                }
                

            }
            return Task.FromResult(isOffline);
        }

        public Task<string[]> GetOnlineUsers()
        {
            string[] onlineUsers=new string []{};
            lock (OnlineUsers)
            {

                onlineUsers = OnlineUsers.OrderBy(x => x.Key).Select(x => x.Key).ToArray();

            }
            
            return Task.FromResult(onlineUsers);
        }

        public Task<List<string>> GetConnectionsForUser(string username)
        {
            List<string> connectionIds = new List<string>();
            lock (OnlineUsers)
            {

                connectionIds= OnlineUsers.GetValueOrDefault(username);
                // HashSet<string> hashSet;
                // if (Connections.TryGetValue(username, out hashSet))
                // {
                //     connectionIds = hashSet.ToList();
                // }
            }
            return Task.FromResult(connectionIds);
        }
    }
}