using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using MvcApplication1.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace MvcApplication1.Hubs
{
    [HubName("OwnerUserHub")]
    public class OwnerUserHub : Hub
    {
        private OwnerUserItemContext OwnerUserItemDatabase = new OwnerUserItemContext();
        private RoomItemContext RoomItemDatabase = new RoomItemContext();

        public bool Register(string username, string password)
        {
            if (OwnerUserItemDatabase.OwnerUserItems.Find(username) == null)
            {
                OwnerUserItemDatabase.OwnerUserItems.Add(new OwnerUserItem()
                {
                    Username = username,
                    Password = password,
                    IsLoggedIn = false,
                    Status = StatusIndicator.Online,
                    Friends = new List<UserItem>(),
                    Rooms = new List<RoomItem>(),
                    Messages = new List<MessageItem>()
                });
                OwnerUserItemDatabase.SaveChanges();
                return true;
            }
            return false;
        }

        public bool Login(string username, string password)
        {
            OwnerUserItem oui = OwnerUserItemDatabase.OwnerUserItems.Find(username);
            if (oui == null || oui.Password != password)
            {
                return false;
            }
            oui.ConnectionId = this.Context.ConnectionId;
            oui.IsLoggedIn = true;
            OwnerUserItemDatabase.SaveChanges();
            return true;
        }

        public bool Logout(string username)
        {
            OwnerUserItem oui = OwnerUserItemDatabase.OwnerUserItems.Find(username);
            if (oui == null)
            {
                return false;
            }
            oui.IsLoggedIn = false;
            oui.Status = StatusIndicator.Offline;
            OwnerUserItemDatabase.SaveChanges();
            return true;
        }

        public void ChangeStatus(string username, StatusIndicator status)
        {
            OwnerUserItem oui = OwnerUserItemDatabase.OwnerUserItems.Find(username);
            if (oui != null)
            {
                oui.Status = status;
                OwnerUserItemDatabase.SaveChanges();
            }
        }

        public bool CreateRoom(string name)
        {
            if (RoomItemDatabase.RoomItems.Find(name) == null)
            {
                RoomItem ri = new RoomItem()
                {
                    Name = name,
                    Users = new List<UserItem>(),
                    Messages = new List<MessageItem>()
                };

                RoomItemDatabase.RoomItems.Add(ri);
                RoomItemDatabase.SaveChanges();

                Clients.Caller.RoomCreated(name);
                return true;
            }
            return false;
        }
    }
}