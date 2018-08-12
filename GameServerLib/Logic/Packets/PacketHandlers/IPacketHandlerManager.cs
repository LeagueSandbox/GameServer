using ENet;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public interface IPacketHandlerManager
    {
        bool BroadcastPacket(byte[] data, Channel channelNo, PacketFlags flag = PacketFlags.Reliable);
        bool BroadcastPacket(PacketDefinitions.Packet packet, Channel channelNo, PacketFlags flag = PacketFlags.Reliable);
        bool BroadcastPacketTeam(TeamId team, byte[] data, Channel channelNo, PacketFlags flag = PacketFlags.Reliable);
        bool BroadcastPacketTeam(TeamId team, PacketDefinitions.Packet packet, Channel channelNo, PacketFlags flag = PacketFlags.Reliable);
        bool BroadcastPacketVision(GameObject o, byte[] data, Channel channelNo, PacketFlags flag = PacketFlags.Reliable);
        bool BroadcastPacketVision(GameObject o, PacketDefinitions.Packet packet, Channel channelNo, PacketFlags flag = PacketFlags.Reliable);
        bool HandlePacket(Peer peer, byte[] data, Channel channelId);
        bool HandlePacket(Peer peer, ENet.Packet packet, Channel channelId);
        bool SendPacket(Peer peer, byte[] source, Channel channelNo, PacketFlags flag = PacketFlags.Reliable);
        bool SendPacket(Peer peer, PacketDefinitions.Packet packet, Channel channelNo, PacketFlags flag = PacketFlags.Reliable);
        void UnpauseGame();
    }
}