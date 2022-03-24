using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace DatingApp.SignalR
{
    public class PresenceHub:Hub
    {
    private readonly PresenceTracker _presenceTracker;
    public PresenceHub( PresenceTracker presenceTracker)
    {
      _presenceTracker = presenceTracker;
       
    }
    

        [Authorize]
        public override async Task OnConnectedAsync()
        {
           var isOnline= await _presenceTracker.UserConnected(Context.User.GetUserName(), Context.ConnectionId);
           if (isOnline)
            await Clients.Others.SendAsync("UserIsOnline", Context.User.GetUserName());

            var currentUser = await _presenceTracker.GetOnlineUsers();
            await Clients.Caller.SendAsync("GetOnlineUsers", currentUser);
            await base.OnConnectedAsync();
        }
        
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var isOffline =await _presenceTracker.UserDisconnected(Context.User.GetUserName(), Context.ConnectionId);

            if (isOffline)
            await Clients.Others.SendAsync("UserIsOffline", Context.User.GetUserName());
            //  var currentUser = await _presenceTracker.GetOnlineUsers();
            // await Clients.All.SendAsync("GetOnlineUsers", currentUser);
            await base.OnDisconnectedAsync(exception);
        }
    }
}