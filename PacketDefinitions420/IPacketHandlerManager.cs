using LENet;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using Channel = GameServerCore.Packets.Enums.Channel;

namespace PacketDefinitions420
{
    /// <summary>
    /// Interface of all functions related to sending and receiving packets.
    /// </summary>
    public interface IPacketHandlerManager
    {
        bool BroadcastPacket(byte[] data, Channel channelNo, PacketFlags flag = PacketFlags.RELIABLE);
        bool BroadcastPacketTeam(TeamId team, byte[] data, Channel channelNo, PacketFlags flag = PacketFlags.RELIABLE);
        bool BroadcastPacketVision(IGameObject o, byte[] data, Channel channelNo, PacketFlags flag = PacketFlags.RELIABLE);
        bool HandlePacket(Peer peer, byte[] data, Channel channelId);
        bool HandlePacket(Peer peer, Packet packet, Channel channelId);
        bool SendPacket(int playerId, byte[] source, Channel channelNo, PacketFlags flag = PacketFlags.RELIABLE);
        bool HandleDisconnect(Peer peer);
    }
}