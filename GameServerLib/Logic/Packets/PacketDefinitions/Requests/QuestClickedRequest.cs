using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.Requests
{
    public class QuestClickedRequest
    {
        public uint QuestNetId { get; }

        public QuestClickedRequest(uint questNetId)
        {
            QuestNetId = questNetId;
        }
    }
}
