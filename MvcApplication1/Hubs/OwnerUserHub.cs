using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using WPChatServer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using MvcApplication1.Models;

namespace WPChatServer.Hubs
{
    [HubName("OwnerUserHub")]
    public class OwnerUserHub : Hub
    {
        private OwnerUserItemContext OwnerUserItemDatabase = new OwnerUserItemContext();
        private RoomItemContext RoomItemDatabase = new RoomItemContext();
        private UserRoomContext UserRoomDatabase = new UserRoomContext();
        private MessageItemContext MessageItemDatabase = new MessageItemContext();

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
                    ConnectionId = this.Context.ConnectionId
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

            return new OwnerUserItem();
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
                };

                RoomItemDatabase.RoomItems.Add(ri);
                RoomItemDatabase.SaveChanges();

                UserRoomDatabase.User_Room.Add(new UserRoomItem()
                {
                    UserId = oui.Username,
                    RoomId = ri.Name
                });
                UserRoomDatabase.SaveChanges();

                return true;
            }
            return false;
        }

        public void SendMessage(MessageItem mi)
        {
            if (mi.Type == DataContextType.User)
            {
                Clients.Client(OwnerUserItemDatabase.OwnerUserItems.Find(mi.To).ConnectionId).ReceiveMessage(mi);
                MessageItemDatabase.MessageItems.Add(new MessageItem() { 
                    From = mi.From,
                    To = mi.To,
                    Type = mi.Type,
                    Text = mi.Text
                });
            }
            else
            {
                List<UserRoomItem> li = UserRoomDatabase.User_Room.Where<UserRoomItem>(x => x.RoomId == mi.To).ToList();

                li.ForEach(x =>
                {
                    if (x.UserId != mi.From)
                    {
                        Clients.Client(OwnerUserItemDatabase.OwnerUserItems.Find(x.UserId).ConnectionId).ReceiveMessage(mi);
                    }
                });

                MessageItemDatabase.MessageItems.Add(new MessageItem()
                {
                    From = mi.From,
                    To = mi.To,
                    Type = mi.Type,
                    Text = mi.Text
                });
            }
        }

        public void AddRoom(string username, string name)
        {
            OwnerUserItem oui = OwnerUserItemDatabase.OwnerUserItems.Find(username);
            RoomItem ri = RoomItemDatabase.RoomItems.Find(name);
            if (oui != null && ri != null)
            {
                UserRoomDatabase.User_Room.Add(new UserRoomItem()
                {
                    UserId = oui.Username,
                    RoomId = ri.Name
                });
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


        public List<RoomItem> GetRoomsByNameStart(string name)
        {
            List<RoomItem> tmp_rooms = new List<RoomItem>();
            RoomItemDatabase.RoomItems.ToList().ForEach(x =>
            {
                if (x.Name.StartsWith(name, StringComparison.CurrentCultureIgnoreCase))
                {
                    tmp_rooms.Add(x);
                }
            });

            return tmp_rooms;
        }

        public List<OwnerUserItem> GetUsersByNameStart(string name)
        {
            List<OwnerUserItem> tmp_users = new List<OwnerUserItem>();
            OwnerUserItemDatabase.OwnerUserItems.ToList().ForEach(x =>
            {
                if (x.Username.StartsWith(name, StringComparison.CurrentCultureIgnoreCase))
                {
                    tmp_users.Add(x);
                }
            });

            return tmp_users;
        }

        public RoomItem GetRoomByName(string name)
        {
            RoomItem room = RoomItemDatabase.RoomItems.Find(name);

            return room;
        }

        public OwnerUserItem GetUserByName(string name)
        {
            OwnerUserItem user = OwnerUserItemDatabase.OwnerUserItems.Find(name);

            return user;
        }
    }
}