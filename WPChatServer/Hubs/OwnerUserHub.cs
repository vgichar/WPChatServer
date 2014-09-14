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
        private FriendRequestContext FriendRequestDatabase = new FriendRequestContext();
        private OwnerUserItem caller
        {
            get
            {
                return OwnerUserItemDatabase.OwnerUserItems.FirstOrDefault(x => x.ConnectionId == this.Context.ConnectionId);
            }
        }

        // Invoke Client-side Code

        private void NotifyFriendStatusChanged(StatusIndicator status)
        {
            foreach (OwnerUserItem friend in caller.Friends)
            {
                if (friend.IsLoggedIn)
                {
                    Clients.Client(friend.ConnectionId).FriendStatusChanged(caller.Username, status);
                }
            }
        }

        private void NotifyFriendRequestReceived(OwnerUserItem receiver)
        {
            if (receiver.IsLoggedIn)
            {
                Clients.Client(receiver.ConnectionId).FriendRequestReceived(caller.Username);
            }
        }

        private void NotifyFriendRequestAccepted(OwnerUserItem sender)
        {
            if (sender.IsLoggedIn)
            {
                Clients.Client(sender.ConnectionId).FriendRequestAccepted(GetUserByName(caller.Username));
            }
            if (caller.IsLoggedIn)
            {
                Clients.Client(caller.ConnectionId).FriendRequestAccepted(GetUserByName(sender.Username));
            }
        }

        private void NotifyAddRoom(RoomItem room)
        {
            foreach (string username in UserRoomDatabase.UserRoomItems.Where(x => x.RoomId == room.Name).Select(x => x.UserId))
            {
                OwnerUserItem member = OwnerUserItemDatabase.OwnerUserItems.Find(username);

                if (member != null && member.IsLoggedIn)
                {
                    Clients.Client(member.ConnectionId).NewMemberInRoom(caller.Username, room.Name);
                }
            }

            foreach (OwnerUserItem friend in caller.Friends)
            {
                if (friend.IsLoggedIn)
                {
                    Clients.Client(friend.ConnectionId).FriendJoinRoom(caller.Username, room.Name);
                }
            }
        }

        private void NotifyRemoveRoom(RoomItem room)
        {
            foreach (string username in UserRoomDatabase.UserRoomItems.Where(x => x.RoomId == room.Name).Select(x => x.UserId))
            {
                OwnerUserItem member = OwnerUserItemDatabase.OwnerUserItems.Find(username);

                if (member != null && member.IsLoggedIn)
                {
                    Clients.Client(member.ConnectionId).RemoveMemberFromRoom(caller.Username, room.Name);
                }
            }

            foreach (OwnerUserItem friend in caller.Friends)
            {
                if (friend.IsLoggedIn)
                {
                    Clients.Client(friend.ConnectionId).FriendLeaveRoom(caller.Username, room.Name);
                }
            }
        }

        // Server-side code

        public bool Register(string username, string password)
        {
            if (OwnerUserItemDatabase.OwnerUserItems.Find(username) == null && RoomItemDatabase.RoomItems.Find(username) == null)
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

            this.NotifyFriendStatusChanged(oui.Status);

            List<object> rooms = new List<object>();
            List<object> friends = new List<object>();
            List<string> friendRequests = new List<string>();

            foreach (string roomName in UserRoomDatabase.UserRoomItems.Where(x => x.UserId == oui.Username).Select(x => x.RoomId))
            {
                rooms.Add(GetRoomByName(roomName));
            }

            foreach (string userName in oui.Friends.Select(x => x.Username))
            {
                friends.Add(GetUserByName(userName));
            }

            foreach (string userName in FriendRequestDatabase.FriendRequests.Where(x => x.UserReceiverName == username).Select(x => x.UserSenderName))
            {
                friendRequests.Add(userName);
            }

            return new
            {
                Username = oui.Username,
                Password = oui.Password,
                IsLoggedIn = oui.IsLoggedIn,
                Status = oui.Status,
                Rooms = rooms,
                Friends = friends,
                FriendRequests = friendRequests
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

            this.NotifyFriendStatusChanged(StatusIndicator.Offline);

            OwnerUserItemDatabase.SaveChanges();
            return true;
        }

        public bool ChangeStatus(StatusIndicator status)
        {
            if (caller != null)
            {
                caller.Status = status;

                this.NotifyFriendStatusChanged(status);
                OwnerUserItemDatabase.SaveChanges();

                return true;
            }
            return false;
        }

        public bool CreateRoom(string name)
        {
            if (caller != null && OwnerUserItemDatabase.OwnerUserItems.Find(name) == null && RoomItemDatabase.RoomItems.Find(name) == null)
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

                NotifyAddRoom(ri);

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

        public object AddRoom(string name)
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

                NotifyAddRoom(ri);

                return GetRoomByName(name);
            }
            return null;
        }

        public bool RemoveRoom(string name)
        {
            RoomItem ri = RoomItemDatabase.RoomItems.Find(name);
            if (caller != null && ri != null)
            {
                UserRoomDatabase.UserRoomItems.Remove(UserRoomDatabase.UserRoomItems.First(x => x.UserId == caller.Username && x.RoomId == ri.Name));
                UserRoomDatabase.SaveChanges();

                if (UserRoomDatabase.UserRoomItems.FirstOrDefault(x => x.RoomId == ri.Name) == null)
                {
                    RoomItemDatabase.RoomItems.Remove(ri);
                    RoomItemDatabase.SaveChanges();
                }

                NotifyRemoveRoom(ri);
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

        public void SendFriendRequest(string toUsername)
        {
            OwnerUserItem sender = caller;
            OwnerUserItem receiver = OwnerUserItemDatabase.OwnerUserItems.Find(toUsername);

            if (sender != null && receiver != null && sender != receiver && sender.Friends.FirstOrDefault(x => x.Username == receiver.Username) == null && FriendRequestDatabase.FriendRequests.FirstOrDefault(x=>x.UserSenderName == sender.Username && x.UserReceiverName == receiver.Username) == null)
            {
                FriendRequestDatabase.FriendRequests.Add(new FriendRequest()
                {
                    UserSenderName = sender.Username,
                    UserReceiverName = receiver.Username
                });

                FriendRequestDatabase.SaveChanges();

                if (receiver.IsLoggedIn)
                {
                    NotifyFriendRequestReceived(receiver);
                }
            }
        }

        public void AcceptFriendRequest(string fromUsername)
        {
            OwnerUserItem sender = OwnerUserItemDatabase.OwnerUserItems.Find(fromUsername);
            OwnerUserItem receiver = caller;

            FriendRequest fr = FriendRequestDatabase.FriendRequests.FirstOrDefault(x => x.UserSenderName == fromUsername && x.UserReceiverName == receiver.Username);

            if (sender != null && receiver != null && sender != receiver && fr != null)
            {
                FriendRequestDatabase.FriendRequests.Remove(fr);
                FriendRequestDatabase.SaveChanges();

                sender.Friends.Add(receiver);
                receiver.Friends.Add(sender);

                OwnerUserItemDatabase.SaveChanges();

                NotifyFriendRequestAccepted(sender);
            }
        }

        public void DenyFriendRequest(string fromUsername)
        {
            OwnerUserItem sender = OwnerUserItemDatabase.OwnerUserItems.Find(fromUsername);
            OwnerUserItem receiver = caller;

            FriendRequest fr = FriendRequestDatabase.FriendRequests.FirstOrDefault(x => x.UserSenderName == fromUsername && x.UserReceiverName == receiver.Username);

            if (sender != null && receiver != null && sender != receiver && fr != null)
            {
                FriendRequestDatabase.FriendRequests.Remove(fr);
                FriendRequestDatabase.SaveChanges();
            }
        }
    }
}