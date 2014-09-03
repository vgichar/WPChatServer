using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WPChatServer.Models
{
    public class FriendRequest
    {
        [Key]
        public int Id { get; set; }
        public string UserSenderName { get; set; }
        public string UserReceiverName { get; set; }
    }
}