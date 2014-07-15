using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcApplication1.Models
{
    public class Initializer<T> : DropCreateDatabaseIfModelChanges<T> where T : DbContext
    {
    }
}
