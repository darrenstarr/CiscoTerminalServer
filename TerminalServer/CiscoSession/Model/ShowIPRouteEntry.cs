using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace TerminalServer.CiscoSession.Model
{
    public enum ERoutingProtocol
    {
        Unspecified,
        Local,
        Connected,
        Static,
        RIP,
        Mobile,
        BGP,
        EIGRP,
        OSPF,
        ISIS,
        ODR,
        PeriodicDownloadStatic,
        NHRP,
        LISP,
        PerUserStatic,
        EIGRPExternal,
        OSPFInterarea,
        OSPFNSSAType1External,
        OSPFNSSAType2External,
        OSPFType1External,
        OSPFType2External,
        ISISSummary,
        ISISLevel1,
        ISISLevel2,
        ISISInterarea
    }

    public class NetworkPrefix
    {
        public IPAddress NetworkAddress { get; set; }
        public int Length { get; set; }

        public static int ClassfulPrefixLength(IPAddress address)
        {
            var bytes = address.GetAddressBytes();
            if ((bytes[0] & 0x80) == 0)
                return 8;
            if ((bytes[0] & 0x40) == 0)
                return 16;
            if ((bytes[0] & 0x20) == 0)
                return 24;
            throw new Exception("Not a valid classful address");
        }

        public override string ToString()
        {
            return NetworkAddress.ToString() + "/" + Length.ToString();
        }

        public static NetworkPrefix FromClassful(IPAddress networkAddress)
        {
            return new NetworkPrefix
            {
                NetworkAddress = networkAddress,
                Length = ClassfulPrefixLength(networkAddress)
            };
        }
    }

    public class ShowIPRouteLastResort
    {
        public IPAddress Gateway { get; set; }
        public IPAddress Network { get; set; }
    }

    public class ShowIPRouteCode
    {
        public bool NextHopOverride { get; set; }
        public ERoutingProtocol Protocol { get; set; }
        public ERoutingProtocol Suffix { get; set; }
        public bool Candidate { get; set; }
        public bool Replicated { get; set; }
    }

    public class NetworkRouteMetric
    {
        public bool DirectlyConnected { get; set; }
        public Int64 AdministrativeDistance { get; set; }
        public Int64 Metric { get; set; }
    }

    public class ShowIPRouteNextHop
    {
        public bool NextHopOverride { get; set; }
        public NetworkRouteMetric RouteMetric { get; set; }
        public IPAddress Via { get; set; }
        public DateTime Uptime { get; set; }
        public CiscoInterfaceId OutgoingInterface { get; set; }
    }

    public class ShowIPRouteEntryItem
    {
        public ShowIPRouteCode Code { get; set; }
        public NetworkPrefix Prefix { get; set; }
        public List<ShowIPRouteNextHop> NextHops { get; set; }
    }

    public class ShowIPRouteEntry
    {
        public ShowIPRouteLastResort LastResort { get; set; }
        public List<ShowIPRouteEntryItem> Routes { get; set; }
    }
}
