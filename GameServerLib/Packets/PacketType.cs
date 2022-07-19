using GameServerCore.Packets.Enums;
using LeaguePackets;
using System;

namespace PacketDefinitions420
{
    [AttributeUsage(AttributeTargets.Method)]
    public class PacketType : Attribute
    {
        public GamePacketID GamePacketId { get; }
        public LoadScreenPacketID LoadScreenPacketId { get; }
        public Channel ChannelId { get; }

        public PacketType(GamePacketID packetId, Channel channel)
        {
            GamePacketId = packetId;
            ChannelId = channel;
        }
        public PacketType(GamePacketID packetId) : this(packetId, Channel.CHL_C2S) { }
        public PacketType(LoadScreenPacketID packetId, Channel channel)
        {
            LoadScreenPacketId = packetId;
            ChannelId = channel;
        }
        public PacketType(LoadScreenPacketID packetId) : this (packetId, Channel.CHL_LOADING_SCREEN) { }
    }
}
