﻿using WPChatServer.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPChatServer.Models
{
    public class RoomItem
    {
        [Key]
        public string Name { get; set; }
    }
}
