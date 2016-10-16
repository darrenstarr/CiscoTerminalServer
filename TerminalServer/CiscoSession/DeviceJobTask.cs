using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TerminalServer.CiscoSession
{
    class DeviceJobResultHandler
    {
        public Regex Expression { get; set; } = null;
        public string NextTask { get; set; } = "";
        public string ErrorString { get; set; } = "";
    }

    class DeviceJobTask
    {
        public string Name { get; set; } = "";
        public string Command { get; set; } = "";
        public List<DeviceJobResultHandler> ResultHandlers = null;
    }
}
