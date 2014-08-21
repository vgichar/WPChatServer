using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using WPChatServer.Models;

namespace WPChatServer.Models
{
    public class UserRoomItemContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
    
        public UserRoomItemContext() : base("name=UserRoomItemContext")
        {
        }

        public System.Data.Entity.DbSet<WPChatServer.Models.UserRoomItem> UserRoomItems { get; set; }
    
    }
}
