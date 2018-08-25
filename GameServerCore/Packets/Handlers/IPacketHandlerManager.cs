using ENet;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Packets.Enums;
using Packet = GameServerCore.Packets.PacketDefinitions.Packet;

namespace GameServerCore.Packets.Handlers
{
    public interface IPacketHandlerManager
    {
        bool BroadcastPacket(byte[] data, Channel channelNo, PacketFlags flag = PacketFlags.Reliable);
        bool BroadcastPacket(Packet packet, Channel channelNo, PacketFlags flag = PacketFlags.Reliable);
        bool BroadcastPacketTeam(TeamId team, byte[] data, Channel channelNo, PacketFlags flag = PacketFlags.Reliable);
        bool BroadcastPacketTeam(TeamId team, PacketDefinitions.Packet packet, Channel channelNo, PacketFlags flag = PacketFlags.Reliable);
        bool BroadcastPacketVision(IGameObject o, byte[] data, Channel channelNo, PacketFlags flag = PacketFlags.Reliable);
        bool BroadcastPacketVision(IGameObject o, PacketDefinitions.Packet packet, Channel channelNo, PacketFlags flag = PacketFlags.Reliable);
        bool HandlePacket(Peer peer, byte[] data, Channel channelId);
        bool HandlePacket(Peer peer, ENet.Packet packet, Channel channelId);
        bool SendPacket(Peer peer, byte[] source, Channel channelNo, PacketFlags flag = PacketFlags.Reliable);
        bool SendPacket(Peer peer, Packet packet, Channel channelNo, PacketFlags flag = PacketFlags.Reliable);
        void UnpauseGame();
    }
}