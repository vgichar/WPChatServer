using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using WPChatServer.Models;

namespace MvcApplication1.Models
{
    public class UserRoomItem
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; }

        public string RoomId { get; set; }
    }
}