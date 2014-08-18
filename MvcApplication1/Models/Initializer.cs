using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WPChatServer.Models
{
    public class Initializer<T> : DropCreateDatabaseAlways<T> where T : DbContext
    {
    }
}
