using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.Requests
{
    public class SynchVersionRequest
    {
        public int NetId { get; }
        public int Unk1 { get; }
        public string Version { get; }

        public SynchVersionRequest(int netId, int unk1, string version)
        {
            NetId = netId;
            Unk1 = unk1;
            Version = version;
        }
    }
}
