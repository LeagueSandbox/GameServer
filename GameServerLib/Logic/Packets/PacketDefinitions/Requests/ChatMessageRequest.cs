using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.Requests
{
    public class ChatMessageRequest
    {
        public string Message { get; }
        public ChatType Type { get; }

        public ChatMessageRequest(string message, ChatType type)
        {
            Message = message;
            Type = type;
        }
    }
}
