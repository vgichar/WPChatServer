using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using WPChatServer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using WPChatServer.Models;

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
                    Status = StatusIndicator.Offline,
                    Friends = new List<OwnerUserItem>(),
                    ConnectionId = this.Context.ConnectionId
                });
                OwnerUserItemDatabase.SaveChanges();
                return true;
            }
            return false;
        }

        public object Login(string username, string password)
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

            List<object> rooms = new List<object>();
            List<object> friends = new List<object>();

            foreach (string roomName in UserRoomDatabase.User_Room.Where(x => x.UserId == oui.Username).Select(x => x.RoomId))
            {
                rooms.Add(GetRoomByName(roomName));
            }

            foreach (string userName in oui.Friends.Select(x => x.Username))
            {
                friends.Add(GetUserByName(userName));
            }

            return new {
                Username = oui.Username,
                Password = oui.Password,
                IsLoggedIn = oui.IsLoggedIn,
                Status = oui.Status,
                Rooms = rooms,
                Friends = friends
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
                OwnerUserItem receiverUser = OwnerUserItemDatabase.OwnerUserItems.Find(mi.To);
                if (receiverUser.IsLoggedIn)
                {
                    Clients.Client(receiverUser.ConnectionId).ReceiveMessage(mi);
                }
            }
            else
            {
                var li = UserRoomDatabase.User_Room.Where<UserRoomItem>(x => x.RoomId == mi.To);

                foreach(var x in li)
                {
                    OwnerUserItem receiverUser = OwnerUserItemDatabase.OwnerUserItems.Find(x.UserId);
                    if (receiverUser.IsLoggedIn && x.UserId != mi.From)
                    {
                        Clients.Client(OwnerUserItemDatabase.OwnerUserItems.Find(x.UserId).ConnectionId).ReceiveMessage(mi);
                    }
                }
            }


            MessageItemDatabase.MessageItems.Add(new MessageItem()
            {
                From = mi.From,
                To = mi.To,
                Type = mi.Type,
                Text = mi.Text
            });
            MessageItemDatabase.SaveChanges();
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
                UserRoomDatabase.SaveChanges();
            }
        }

        public void AddFriend(string username, string friend)
        {
            OwnerUserItem oui = OwnerUserItemDatabase.OwnerUserItems.Find(username);
            OwnerUserItem friend_oui = OwnerUserItemDatabase.OwnerUserItems.Find(friend);

            if (oui != null && friend_oui != null)
            {
                oui.Friends.Add(friend_oui);
                OwnerUserItemDatabase.SaveChanges();
            }
        }

        public void RemoveRoom(string username, string name)
        {
            OwnerUserItem oui = OwnerUserItemDatabase.OwnerUserItems.Find(username);
            RoomItem ri = RoomItemDatabase.RoomItems.Find(name);
            if (oui != null && ri != null)
            {
                UserRoomDatabase.User_Room.Remove(UserRoomDatabase.User_Room.First(x=>x.UserId == username && x.RoomId == name));
                UserRoomDatabase.SaveChanges();
            }
        }

        public void RemoveFriend(string username, string friend)
        {
            OwnerUserItem oui = OwnerUserItemDatabase.OwnerUserItems.Find(username);
            OwnerUserItem friend_oui = OwnerUserItemDatabase.OwnerUserItems.Find(friend);

            if (oui != null && friend_oui != null)
            {
                oui.Friends.Remove(friend_oui);
                OwnerUserItemDatabase.SaveChanges();
            }
        }


        public List<RoomItem> GetRoomsByNameStart(string username, string name)
        {
            OwnerUserItem oui = OwnerUserItemDatabase.OwnerUserItems.Find(username);
            List<RoomItem> tmp_rooms = new List<RoomItem>();

            RoomItemDatabase.RoomItems.ToList().ForEach(x =>
            {
                if (x.Name.StartsWith(name, StringComparison.CurrentCultureIgnoreCase) && UserRoomDatabase.User_Room.FirstOrDefault(y=>y.UserId == oui.Username && y.RoomId == x.Name) == null)
                {
                    tmp_rooms.Add(new RoomItem()
                    {
                        Name = x.Name
                    });
                }
            });

            return tmp_rooms;
        }

        public List<OwnerUserItem> GetUsersByNameStart(string username, string name)
        {
            OwnerUserItem oui = OwnerUserItemDatabase.OwnerUserItems.Find(username);
            List<OwnerUserItem> tmp_users = new List<OwnerUserItem>();

            OwnerUserItemDatabase.OwnerUserItems.ToList().ForEach(x =>
            {
                if (x.Username.StartsWith(name, StringComparison.CurrentCultureIgnoreCase) && oui.Friends.FirstOrDefault(y => y.Username == x.Username) == null && x.Username != oui.Username)
                {
                    tmp_users.Add(new OwnerUserItem()
                    {
                        Username = x.Username
                    });
                }
            });

            return tmp_users;
        }

        public object GetRoomByName(string name)
        {
            RoomItem room = RoomItemDatabase.RoomItems.Find(name);

            List<object> users = new List<object>();

            foreach (string userName in UserRoomDatabase.User_Room.Where(x=> x.RoomId == room.Name).Select(x=>x.UserId))
            {
                users.Add(new
                {
                    Username = userName
                });
            }

            return new {
                Name = room.Name,
                Users = users,
                Messages = MessageItemDatabase.MessageItems.Where(x => x.From == room.Name || x.To == room.Name)
            };
        }

        public object GetUserByName(string name)
        {
            OwnerUserItem user = OwnerUserItemDatabase.OwnerUserItems.Find(name);

            List<object> rooms = new List<object>();

            foreach(string roomName in UserRoomDatabase.User_Room.Where(x => x.UserId == user.Username).Select(x => x.RoomId)){
                rooms.Add(new {
                    Name = roomName
                });
            }

            return new
            {
                Username = user.Username,
                Status = user.Status,
                Rooms = rooms,
                Messages = MessageItemDatabase.MessageItems.Where(x=> x.From == user.Username || x.To == user.Username)
            };
        }
    }
}