using GameServerCore.Packets.Enums;
using System;

namespace PacketDefinitions420
{
    [AttributeUsage(AttributeTargets.Method)]
    public class PacketType : Attribute
    {
        public PacketCmd PacketId { get; }
        public Channel ChannelId { get; }

        public PacketType(PacketCmd packetId, Channel channel = Channel.CHL_C2S)
        {
            PacketId = packetId;
            ChannelId = channel;
        }
    }
}
