using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MvcApplication1.Models
{
    public class MessageItem
    {
        [Key]
        public int Id { get; set; }

        public DataContextType Type { get; set; }

        public string From { get; set; }

        public string To { get; set; }

        public string Text { get; set; }

        public object Link { get; set; }
    }
}
