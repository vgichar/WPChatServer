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
    public class RoomItem
    {
        [Key]
        public string Name { get; set; }

        public List<UserItem> Users { get; set; }

        public List<MessageItem> Messages { get; set; }
    }
}
