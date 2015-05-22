using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpawmetDatabase
{
    public class ParserRunCompletedEventArgs : EventArgs
    {
        public TimeSpan TimeElapsed { get; set; }

        public ParserRunCompletedEventArgs(TimeSpan elapsed)
        {
            TimeElapsed = elapsed;
        }
    }
}
