using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalServer.CiscoSession
{
    public enum CiscoShowType
    {
        NotSet = 0,
        Running = 1,
        Startup = 2,
        Version = 3,
        Inventory = 4,
        Diag = 5,
        Lines = 6
    }
}