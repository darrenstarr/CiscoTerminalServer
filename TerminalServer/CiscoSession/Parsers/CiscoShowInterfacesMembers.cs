﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

using TerminalServer.CiscoSession.Model;

namespace TerminalServer.CiscoSession.Parsers
{
    partial class CiscoShowInterfaces
    {
        CiscoInterfaceId ToCiscoInterfaceId(EInterfaceType interfaceType, CiscoInterfaceNumber interfaceNumber, IList<int>channelNumber, IList<int>subinterfaceNumber)
        {
            var result = new CiscoInterfaceId
            {
                InterfaceType = interfaceType,
                InterfaceNumber = interfaceNumber
            };

            if (channelNumber.Count > 0)
                result.InterfaceNumber.Channel = channelNumber[0];

            if (subinterfaceNumber.Count > 0)
                result.InterfaceNumber.Subinterface = subinterfaceNumber[0];

            return result;
        }

        DateTime CiscoTimeToDateTime(IList<int> seconds)
        {
            int total = seconds.Sum();
            return DateTime.Now.AddSeconds(-(total));
        }

        int CiscoTimeToSeconds(int x, string units)
        {
            switch (units)
            {
                case "y":
                    return 31536000 * x;
                case "w":
                    return 86400 * 7 * x;
                case "d":
                    return 86400 * x;
                case "h":
                    return 3600 * x;
                case "m":
                    return 60 * x;
                case "s":
                    return x;
            }
            throw new Exception("Unrecognized time unit '" + units + "'");
        }

        public PhysicalAddress ParseCiscoMACAddress(string address)
        {
            return PhysicalAddress.Parse(address.Replace(".", "").ToUpper());
        }

        public Int64 ExpandBandwidth(Int64 n, Int64 rate)
        {
            return n * rate;
        }
    }
}
