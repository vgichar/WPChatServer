using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPChatServer.Models
{
    public class MessageItem
    {
        [Key]
        public int Id { get; set; }

        public DataContextType Type { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public string Text { get; set; }
    }
}
