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
        private UserRoomItemContext UserRoomDatabase = new UserRoomItemContext();
        private MessageItemContext MessageItemDatabase = new MessageItemContext();
        private OwnerUserItem caller
        {
            get
            {
                return OwnerUserItemDatabase.OwnerUserItems.FirstOrDefault(x => x.ConnectionId == this.Context.ConnectionId);
            }
        }

        // Invoke Client-side Code

        private void NotifyChangeStatus(StatusIndicator status)
        {
            foreach (OwnerUserItem friend in caller.Friends)
            {
                if (friend.IsLoggedIn)
                {
                    Clients.Client(friend.ConnectionId).FriendStatusChanged(caller.Username, status);
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

            OwnerUserItemDatabase.SaveChanges();

            this.NotifyChangeStatus(oui.Status);

            List<object> rooms = new List<object>();
            List<object> friends = new List<object>();

            foreach (string roomName in UserRoomDatabase.UserRoomItems.Where(x => x.UserId == oui.Username).Select(x => x.RoomId))
            {
                rooms.Add(GetRoomByName(roomName));
            }

            foreach (string userName in oui.Friends.Select(x => x.Username))
            {
                friends.Add(GetUserByName(userName));
            }

            return new
            {
                Username = oui.Username,
                Password = oui.Password,
                IsLoggedIn = oui.IsLoggedIn,
                Status = oui.Status,
                Rooms = rooms,
                Friends = friends
            };
        }
        
        public bool Logout()
        {
            if (caller == null)
            {
                return false;
            }
            caller.IsLoggedIn = false;
            caller.ConnectionId = null;

            this.NotifyChangeStatus(StatusIndicator.Offline);

            OwnerUserItemDatabase.SaveChanges();
            return true;
        }

        public void ChangeStatus(StatusIndicator status)
        {
            if (caller != null)
            {
                caller.Status = status;

                this.NotifyChangeStatus(status);

                OwnerUserItemDatabase.SaveChanges();
            }
        }

        public bool CreateRoom(string name)
        {
            if (caller != null && RoomItemDatabase.RoomItems.Find(name) == null)
            {

                RoomItem ri = new RoomItem()
                {
                    Name = name,
                };

                RoomItemDatabase.RoomItems.Add(ri);
                RoomItemDatabase.SaveChanges();

                UserRoomDatabase.UserRoomItems.Add(new UserRoomItem()
                {
                    UserId = caller.Username,
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
                var li = UserRoomDatabase.UserRoomItems.Where<UserRoomItem>(x => x.RoomId == mi.To);

                foreach (var x in li)
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

        public void AddRoom(string name)
        {
            RoomItem ri = RoomItemDatabase.RoomItems.Find(name);
            if (caller != null && ri != null)
            {
                UserRoomDatabase.UserRoomItems.Add(new UserRoomItem()
                {
                    UserId = caller.Username,
                    RoomId = ri.Name
                });
                UserRoomDatabase.SaveChanges();
            }
        }

        public void AddFriend(string friend)
        {
            OwnerUserItem friend_oui = OwnerUserItemDatabase.OwnerUserItems.Find(friend);

            if (caller != null && friend_oui != null)
            {
                caller.Friends.Add(friend_oui);
                OwnerUserItemDatabase.SaveChanges();
            }
        }

        public bool RemoveRoom(string name)
        {
            RoomItem ri = RoomItemDatabase.RoomItems.Find(name);
            if (caller != null && ri != null)
            {
                UserRoomDatabase.UserRoomItems.Remove(UserRoomDatabase.UserRoomItems.First(x => x.UserId == caller.Username && x.RoomId == ri.Name));
                UserRoomDatabase.SaveChanges();

                if (UserRoomDatabase.UserRoomItems.FirstOrDefault(x => x.RoomId == ri.Name) == null) {
                    RoomItemDatabase.RoomItems.Remove(ri);
                    RoomItemDatabase.SaveChanges();
                }
                return true;
            }
            return false;
        }

        public bool RemoveFriend(string friend)
        {
            OwnerUserItem friend_oui = OwnerUserItemDatabase.OwnerUserItems.Find(friend);

            if (caller != null && friend_oui != null)
            {
                caller.Friends.Remove(friend_oui);
                OwnerUserItemDatabase.SaveChanges();
                return true;
            }
            return false;
        }


        public List<RoomItem> GetRoomsByNameStart(string name)
        {
            List<RoomItem> tmp_rooms = new List<RoomItem>();

            RoomItemDatabase.RoomItems.ToList().ForEach(x =>
            {
                if (x.Name.StartsWith(name, StringComparison.CurrentCultureIgnoreCase) && UserRoomDatabase.UserRoomItems.FirstOrDefault(y => y.UserId == caller.Username && y.RoomId == x.Name) == null)
                {
                    tmp_rooms.Add(new RoomItem()
                    {
                        Name = x.Name
                    });
                }
            });

            return tmp_rooms;
        }

        public List<OwnerUserItem> GetUsersByNameStart(string name)
        {
            List<OwnerUserItem> tmp_users = new List<OwnerUserItem>();

            OwnerUserItemDatabase.OwnerUserItems.ToList().ForEach(x =>
            {
                if (x.Username.StartsWith(name, StringComparison.CurrentCultureIgnoreCase) && caller.Friends.FirstOrDefault(y => y.Username == x.Username) == null && x.Username != caller.Username)
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

            foreach (string userName in UserRoomDatabase.UserRoomItems.Where(x => x.RoomId == room.Name && caller.Username != x.UserId).Select(x => x.UserId))
            {
                users.Add(new
                {
                    Username = userName
                });
            }

            return new
            {
                Name = room.Name,
                Users = users,
                Messages = MessageItemDatabase.MessageItems.Where(x => (x.From == room.Name || x.To == room.Name) && x.Type == Models.DataContextType.Room)
            };
        }

        public object GetUserByName(string name)
        {
            OwnerUserItem user = OwnerUserItemDatabase.OwnerUserItems.Find(name);

            List<object> rooms = new List<object>();

            foreach (string roomName in UserRoomDatabase.UserRoomItems.Where(x => x.UserId == user.Username).Select(x => x.RoomId))
            {
                rooms.Add(new
                {
                    Name = roomName
                });
            }

            return new
            {
                Username = user.Username,
                Status = user.Status,
                Rooms = rooms,
                Messages = MessageItemDatabase.MessageItems.Where(x => (x.From == user.Username || x.To == user.Username) && x.Type == Models.DataContextType.User)
            };
        }

        public void FriendRequest(string from, string username)
        {
            /* the actual code
            OwnerUserItem receiverUser = OwnerUserItemDatabase.OwnerUserItems.Find(username);
            if (receiverUser.IsLoggedIn)
            {
                Debugger.Log(1, "request", username);
                Clients.Client(receiverUser.ConnectionId).FriendRequestRecieve(from);
            }
            */
            //mock
            OwnerUserItem receiverUser = OwnerUserItemDatabase.OwnerUserItems.Find(from);
            if (receiverUser.IsLoggedIn)
            {
            Debugger.Log(1, "request", "asd");
                Clients.Client(receiverUser.ConnectionId).FriendRequestRecieve(from);
            }
        }

        public void FriendAccept(string from, string username)
        {
            
        }

    }
}