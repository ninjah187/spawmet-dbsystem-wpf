﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawmetDatabase.Model
{
    public class User
    {
        public User()
        {
            
        }

        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public UserGroup Group { get; set; }
    }
}
