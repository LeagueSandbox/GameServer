using GameServerCore.Packets.Enums;
using PacketDefinitions420.PacketDefinitions;
using System;

namespace PacketDefinitions420
{
    [AttributeUsage(AttributeTargets.Method)]
    public class PacketType : Attribute
    {
        public PacketCmd PacketId { get; }
        public Channel ChannelId { get; }

        public PacketType(PacketCmd packetId, Channel channel)
        {
            PacketId = packetId;
            ChannelId = channel;
        }
        public PacketType(PacketCmd packetId) : this(packetId, Channel.CHL_C2S) { }
    }
}
