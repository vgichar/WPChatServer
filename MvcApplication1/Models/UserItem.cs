using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcApplication1.Models
{
    public class UserItem
    {
        [Key]
        public string Username { get; set; }

        public StatusIndicator Status { get; set; }

        public List<RoomItem> Rooms { get; set; }

        public List<MessageItem> Messages { get; set; }
    }
}
