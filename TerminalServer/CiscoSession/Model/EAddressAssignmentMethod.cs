﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalServer.CiscoSession.Model
{
    public enum EAddressAssignmentMethod
    {
        NVRAM,
        DHCP,
        Unset,
        Manual,
        SetupCommand
    }
}
