using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Novacode;
using SpawmetDatabase.Model;

namespace SpawmetDatabase.FileCreators
{
    public abstract class Creator
    {
        protected Formatting _footerFormatting;
        protected Formatting _machineFormatting;

        protected Creator()
        {
            _footerFormatting = new Formatting()
            {
                FontFamily = new FontFamily("Arial"),
                Size = 8,
                Position = 5,
            };

            _machineFormatting = new Formatting()
            {
                FontFamily = new FontFamily("Arial Black"),
                Size = 14,
                Position = 0
            };
        }

        public abstract void Create(Machine machine, string savePath);
        public abstract void Create(IEnumerable<Machine> machines, string savePath);
        public abstract void Create(Order order, string savePath);
        public abstract void Create(IEnumerable<Order> orders, string savePath);
        public abstract void Create(Part part, string savePath);
        public abstract void Create(IEnumerable<Part> parts, string savePath);
    }
}
