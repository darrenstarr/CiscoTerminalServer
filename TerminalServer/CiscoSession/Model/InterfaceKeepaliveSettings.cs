﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalServer.CiscoSession.Model
{
    public class InterfaceKeepaliveSettings
    {
        public EInterfaceKeepalive State { get; set; }
        public int Interval { get; set; }
    }
}
