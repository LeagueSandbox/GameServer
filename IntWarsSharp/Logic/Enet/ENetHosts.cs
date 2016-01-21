using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntWarsSharp.Core.Logic.Enet
{
    public enum ENetHosts : uint
    {
        ENET_HOST_ANY = 0,            /**< specifies the default server host */
        ENET_HOST_BROADCAST = 0xFFFFFFFF,   /**< specifies a subnet-wide broadcast */
        ENET_PORT_ANY = 0             /**< specifies that a port should be automatically chosen */
    };
}
