using ENet;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420
{
    public interface IPacketHandlerManager
    {
        bool BroadcastPacket(byte[] data, Channel channelNo, PacketFlags flag = PacketFlags.Reliable);
        bool BroadcastPacket(Packet packet, Channel channelNo, PacketFlags flag = PacketFlags.Reliable);
        bool BroadcastPacketTeam(TeamId team, byte[] data, Channel channelNo, PacketFlags flag = PacketFlags.Reliable);
        bool BroadcastPacketTeam(TeamId team, Packet packet, Channel channelNo, PacketFlags flag = PacketFlags.Reliable);
        bool BroadcastPacketVision(IGameObject o, byte[] data, Channel channelNo, PacketFlags flag = PacketFlags.Reliable);
        bool BroadcastPacketVision(IGameObject o, Packet packet, Channel channelNo, PacketFlags flag = PacketFlags.Reliable);
        bool HandlePacket(Peer peer, byte[] data, Channel channelId);
        bool HandlePacket(Peer peer, ENet.Packet packet, Channel channelId);
        bool SendPacket(int userId, byte[] source, Channel channelNo, PacketFlags flag = PacketFlags.Reliable);
        bool SendPacket(int userId, Packet packet, Channel channelNo, PacketFlags flag = PacketFlags.Reliable);
        // TODO: is this really should be in PacketHandler?
        void UnpauseGame();
        bool HandleDisconnect(Peer peer);
    }
}