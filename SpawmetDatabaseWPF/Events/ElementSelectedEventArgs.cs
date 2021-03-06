﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpawmetDatabase.Model;

namespace SpawmetDatabaseWPF.Events
{
    public class ElementSelectedEventArgs<T>
    {
        public T Element { get; private set; }

        public ElementSelectedEventArgs(T element)
        {
            Element = element;
        }
    }
}
