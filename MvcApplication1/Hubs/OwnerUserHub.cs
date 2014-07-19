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

        private OwnerUserItem User = null;

        // Invoke Client-side Code

        private void NotifyChangeStatus(string username, StatusIndicator status)
        {
            OwnerUserItem oui = OwnerUserItemDatabase.OwnerUserItems.Find(username);
            foreach (OwnerUserItem friend in oui.Friends)
            {
                if (friend.IsLoggedIn)
                {
                    Clients.Client(friend.ConnectionId).FriendStatusChanged(username, status);
                }
            }
        }

        private void onUserStatus()
        {
            Clients.Caller.onUserStatus(User.Status);
        }

        private void onUserFriends()
        {
            List<OwnerUserItem> result = new List<OwnerUserItem>();
            foreach (OwnerUserItem oui in User.Friends) {
                result.Add(new OwnerUserItem()
                {
                    Username = oui.Username,
                    Status = oui.Status,
                });
            }
            Clients.Caller.onUserFriends(result);
        }

        private void onFriendsRooms(OwnerUserItem friend)
        {
            List<RoomItem> rooms = new List<RoomItem>();
            foreach (RoomItem ri in friend.Rooms)
            {
                rooms.Add(new RoomItem()
                {
                    Name = ri.Name
                });
            }
            Clients.Caller.onFriendsRooms(friend.Username, rooms);
        }

        private void onUserRooms()
        {
            List<RoomItem> result = new List<RoomItem>();
            foreach (RoomItem ri in User.Rooms)
            {
                result.Add(new RoomItem()
                {
                    Name = ri.Name,
                    Messages = ri.Messages,
                    Users = new List<OwnerUserItem>(ri.Users.Count)
                });
            }
            Clients.Caller.onUserRooms(result);
        }

        private void onRoomsUsers(RoomItem room)
        {
            List<OwnerUserItem> users = new List<OwnerUserItem>();
            foreach (OwnerUserItem ui in room.Users)
            {
                users.Add(new OwnerUserItem()
                {
                    Username = ui.Username,
                    Status = ui.Status,
                    Rooms = new List<RoomItem>(ui.Rooms.Capacity),
                    Messages = new List<MessageItem>();
                });
            }
            Clients.Caller.onFriendsRooms(room.Name, users);
        }

        private void onUserMessages()
        {
            List<MessageItem> result = new List<MessageItem>();
            foreach (MessageItem mi in User.Messages)
            {
                result.Add(new MessageItem()
                {
                    From = mi.From,
                    To = mi.To,
                    Text = mi.Text,
                    Type = mi.Type,
                    Link = mi.Link
                });
            }
            Clients.Caller.onUserMessages(result);
        }

        // Server-side code

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
                    Friends = new List<OwnerUserItem>(),
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

            this.NotifyChangeStatus(username, oui.Status);

            OwnerUserItemDatabase.SaveChanges();

            User = oui;

            onUserStatus();
            onUserFriends();
            onUserRooms();
            onUserMessages();

            return true;
        }

        public bool Logout()
        {
            if (User == null)
            {
                return false;
            }
            User.IsLoggedIn = false;

            this.NotifyChangeStatus(User.Username, StatusIndicator.Offline);

            OwnerUserItemDatabase.SaveChanges();
            return true;
        }

        public void ChangeStatus(StatusIndicator status)
        {
            if (User != null)
            {
                User.Status = status;

                this.NotifyChangeStatus(User.Username, status);

                OwnerUserItemDatabase.SaveChanges();
            }
        }

        public bool CreateRoom(string name)
        {
            if (User != null && RoomItemDatabase.RoomItems.Find(name) == null)
            {
                RoomItem ri = new RoomItem()
                {
                    Name = name,
                    Users = new List<OwnerUserItem>(),
                    Messages = new List<MessageItem>()
                };

                RoomItemDatabase.RoomItems.Add(ri);
                RoomItemDatabase.SaveChanges();

                User.Rooms.Add(ri);

                OwnerUserItemDatabase.SaveChanges();

                return true;
            }
            return false;
        }
    }
}