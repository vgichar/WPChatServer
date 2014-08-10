using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using WPChatServer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace WPChatServer.Hubs
{
    [HubName("OwnerUserHub")]
    public class OwnerUserHub : Hub
    {
        private OwnerUserItemContext OwnerUserItemDatabase = new OwnerUserItemContext();
        private RoomItemContext RoomItemDatabase = new RoomItemContext();

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
                    Messages = new List<MessageItem>(),
                    FavouriteRooms = new List<RoomItem>()
                });
                OwnerUserItemDatabase.SaveChanges();
                return true;
            }
            return false;
        }

        public OwnerUserItem Login(string username, string password)
        {
            OwnerUserItem oui = OwnerUserItemDatabase.OwnerUserItems.Find(username);
            if (oui == null || oui.Password != password)
            {
                return null;
            }
            oui.ConnectionId = this.Context.ConnectionId;
            oui.IsLoggedIn = true;

            this.NotifyChangeStatus(username, oui.Status);

            OwnerUserItemDatabase.SaveChanges();

            List<RoomItem> rooms = new List<RoomItem>();
            List<OwnerUserItem> friends = new List<OwnerUserItem>();
            List<RoomItem> favouriterooms = new List<RoomItem>();

            oui.FavouriteRooms.ForEach(x =>
            {
                List<OwnerUserItem> users = new List<OwnerUserItem>();

                x.Users.ForEach(y =>
                {
                    users.Add(new OwnerUserItem()
                    {
                        Username = y.Username
                    });
                });

                favouriterooms.Add(new RoomItem()
                {
                    Name = x.Name,
                    Messages = x.Messages,
                    Users = users
                });
            });

            oui.Rooms.ForEach(x => {
                List<OwnerUserItem> users = new List<OwnerUserItem>();
                
                x.Users.ForEach(y =>
                {
                    users.Add(new OwnerUserItem()
                    {
                        Username = y.Username
                    });
                });

                rooms.Add(new RoomItem() {
                    Name = x.Name,
                    Messages = x.Messages,
                    Users = users
                });
            });

            oui.Friends.ForEach(x =>
            {
                List<RoomItem> r = new List<RoomItem>();
                List<MessageItem> m = new List<MessageItem>();

                x.Rooms.ForEach(y =>
                {
                    r.Add(new RoomItem() {
                        Name = y.Name
                    });
                });

                oui.Messages.ForEach(y =>
                {
                    string friendName = y.From;
                    if (y.From == oui.Username)
                    {
                        friendName = y.To;
                    }

                    if (friendName == x.Username)
                    {
                        m.Add(y);
                    }
                });

                friends.Add(new OwnerUserItem() {
                    Username = x.Username,
                    Status = x.Status,
                    Rooms = r
                });
            });

            return new OwnerUserItem() {
                Username = username,
                Password = password,
                IsLoggedIn = true,
                Status = oui.Status,
                Rooms = rooms,
                Friends = friends,
                FavouriteRooms = favouriterooms
            };
        }

        public bool Logout(string username)
        {
            OwnerUserItem oui = OwnerUserItemDatabase.OwnerUserItems.Find(username);
            if (oui == null)
            {
                return false;
            }
            oui.IsLoggedIn = false;

            this.NotifyChangeStatus(oui.Username, StatusIndicator.Offline);

            OwnerUserItemDatabase.SaveChanges();
            return true;
        }

        public void ChangeStatus(string username, StatusIndicator status)
        {
            OwnerUserItem oui = OwnerUserItemDatabase.OwnerUserItems.Find(username);
            if (oui != null)
            {
                oui.Status = status;

                this.NotifyChangeStatus(oui.Username, status);

                OwnerUserItemDatabase.SaveChanges();
            }
        }

        public bool CreateRoom(string username, string name)
        {
            OwnerUserItem oui = OwnerUserItemDatabase.OwnerUserItems.Find(username);
            if (oui != null && RoomItemDatabase.RoomItems.Find(name) == null)
            {
                RoomItem ri = new RoomItem()
                {
                    Name = name,
                    Users = new List<OwnerUserItem>(),
                    Messages = new List<MessageItem>()
                };

                RoomItemDatabase.RoomItems.Add(ri);
                RoomItemDatabase.SaveChanges();

                oui.Rooms.Add(ri);
                OwnerUserItemDatabase.SaveChanges();

                RoomItemDatabase.RoomItems.Find(name).Users.Add(oui);
                RoomItemDatabase.SaveChanges();

                return true;
            }
            return false;
        }

        public void SendMessage(MessageItem mi) {
            if (mi.Type == DataContextType.User) {
                Clients.Client(OwnerUserItemDatabase.OwnerUserItems.Find(mi.To).ConnectionId).ReceiveMessage(mi);
            }
            else
            {
                RoomItem ri = RoomItemDatabase.RoomItems.Find(mi.To);

                Debug.WriteLine(ri.Name);

                ri.Users.ForEach(x =>
                {
                    Debug.WriteLine(x.Username);
                    Clients.Client(x.ConnectionId).ReceiveMessage(mi);
                });
            }
        }

        public void AddRoom(string username ,string name)
        {
            OwnerUserItem oui = OwnerUserItemDatabase.OwnerUserItems.Find(username);
            RoomItem ri = RoomItemDatabase.RoomItems.Find(name);
            if (oui != null && ri != null)
            {
                oui.FavouriteRooms.Add(ri);
                OwnerUserItemDatabase.SaveChanges();
            }
        }

        public void AddFriend(string username, string friend)
        {
            OwnerUserItem oui = OwnerUserItemDatabase.OwnerUserItems.Find(username);
            OwnerUserItem friend_oui = OwnerUserItemDatabase.OwnerUserItems.Find(friend);

            oui.Friends.Add(friend_oui);
            OwnerUserItemDatabase.SaveChanges();
        }

    }
}