using ENet;
using LeagueSandbox.GameServer.Logic.Packets;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleUseObject : IPacketHandler
    {
        public bool HandlePacket(Peer peer, byte[] data, Game game)
        {
            var parsedData = new UseObject(data);
            Logger.LogCoreInfo("Object " + game.GetPeerInfo(peer).GetChampion().getNetId() + " is trying to use (right clicked) " + parsedData.targetNetId);

            return true;
        }
    }
}
