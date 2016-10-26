using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CiscoCLIParsers.Model;

namespace CiscoCLIParsers.Parsers
{
    public partial class CiscoShowIPInterface
    {
        CiscoInterfaceId ToCiscoInterfaceId(EInterfaceType interfaceType, CiscoInterfaceNumber interfaceNumber, IList<int> channelNumber, IList<int> subinterfaceNumber)
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
    }
}
