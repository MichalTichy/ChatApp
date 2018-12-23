using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChatApp.WEB.DAL;
using ChatApp.WEB.Services;
using ChatApp.WEB.ViewModels;
using Microsoft.AspNetCore.SignalR;

namespace ChatApp.WEB.Hub
{
    
    public class ChatHub : Microsoft.AspNetCore.SignalR.Hub
    {
        public static Dictionary<string,string> ConnectedUsers = new Dictionary<string, string>();
        
        public static bool IsUserConnected(string userName)
        {
            return ConnectedUsers.ContainsKey(userName);
        }
        public override Task OnConnectedAsync()
        {
            var username = Context.User.Identity.Name;

            if (!ConnectedUsers.ContainsKey(username))
            {
                ConnectedUsers.Add(username,Context.ConnectionId);
            }
            else
            {
                ConnectedUsers[username] = Context.ConnectionId;
            }

            Clients.Others.SendAsync("ClientConnected", username);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            var username = Context.User.Identity.Name;

            if (ConnectedUsers.ContainsKey(username))
            {
                ConnectedUsers.Remove(username);
            }


            Clients.Others.SendAsync("ClientDisconnected", username);
            return base.OnDisconnectedAsync(exception);
        }
        
    }
}
