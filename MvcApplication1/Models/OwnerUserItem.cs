using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcApplication1.Models
{
    public class OwnerUserItem
    {
        [Key]
        public string Username { get; set; }

        public string Password { get; set; }

        public bool IsLoggedIn { get; set; }

        public StatusIndicator Status { get; set; }

        public List<UserItem> Friends { get; set; }

        public List<RoomItem> Rooms { get; set; }

        public List<MessageItem> Messages { get; set; }

        public string ConnectionId { get; set; }
    }
}
