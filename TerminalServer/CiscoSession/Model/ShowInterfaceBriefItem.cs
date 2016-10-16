using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace TerminalServer.CiscoSession.Model
{
    public enum EInterfaceType
    {
        Ethernet,
        FastEthernet,
        GigabitEthernet,
        TenGigabitEthernet,
        VLan,
        PortChannel,
        Loopback,
        Tunnel,
        Async,
        NVI,
        UCSE,
        EmbeddedServicesEngine
    }

    public enum EInterfaceStatus
    {
        AdministrativelyDown,
        Down,
        Up
    }

    public enum EAddressAssignmentMethod
    {
        NVRAM,
        DHCP,
        Unset,
        Manual
    }

    public class CiscoInterfaceNumber
    {
        public int Chassis { get; set; } = -1;
        public int LineCard { get; set; } = -1;
        public int Module { get; set; } = -1;
        public int Port { get; set; } = -1;

        public override string ToString()
        {
            if (Chassis >= 0)
                return Chassis.ToString() + "/" + LineCard.ToString() + "/" + Module.ToString() + "/" + Port.ToString();
            if (LineCard >= 0)
                return LineCard.ToString() + "/" + Module.ToString() + "/" + Port.ToString();
            if (Module >= 0)
                return Module.ToString() + "/" + Port.ToString();
            if (Port >= 0)
                return Port.ToString();
            return "#Unassigned#";
        }
    }

    public class CiscoInterfaceId
    {
        public EInterfaceType InterfaceType { get; set; }
        public CiscoInterfaceNumber InterfaceNumber { get; set; }

        public override string ToString()
        {
            return InterfaceType.ToString() + InterfaceNumber.ToString();
        }
    }

    public class ShowInterfaceBriefItem
    {
        public CiscoInterfaceId InterfaceId { get; set; } = null;
        public IPAddress Address { get; set; } = null;
        public bool Ok { get; set; }
        public EAddressAssignmentMethod Method { get; set; }
        public EInterfaceStatus Status { get; set; }
        public EInterfaceStatus ProtocolStatus { get; set; }

        public override string ToString()
        {
            return
                InterfaceId.ToString() + " - " +
                (Address == null ? "unassigned" : Address.ToString()) + " - " +
                Ok.ToString() + " - " +
                Method.ToString() + " - " +
                Status.ToString() + " - " +
                ProtocolStatus.ToString();
        }
    }
}
