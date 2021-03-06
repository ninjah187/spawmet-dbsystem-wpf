﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawmetDatabaseWPF.Services
{
    public interface IFactory<T> where T : new()
    {
        T GetItem();
    }
}
