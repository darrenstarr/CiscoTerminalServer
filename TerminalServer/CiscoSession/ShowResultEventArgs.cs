using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalServer.CiscoSession
{
    public class ShowResultEventArgs : EventArgs
    {
        public CiscoShowType ShowType { get; set; }

        public string Text { get; set; }
    }
}
