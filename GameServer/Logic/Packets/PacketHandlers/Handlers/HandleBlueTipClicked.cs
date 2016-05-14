using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Packets;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleBlueTipClicked : IPacketHandler
    {
        public bool HandlePacket(Peer peer, byte[] data, Game game)
        {
            var blueTipClicked = new BlueTipClicked(data);
            var removeBlueTip = new BlueTip("", "", game.GetPeerInfo(peer).GetChampion().getNetId(), blueTipClicked.netid, true);
            game.PacketHandlerManager.sendPacket(peer, removeBlueTip, Channel.CHL_S2C);

            game.ChatboxManager.SendDebugMsgFormatted(GameServer.Logic.Chatbox.ChatboxManager.DebugMsgType.NORMAL, "Clicked blue tip with netid: " + blueTipClicked.netid.ToString());
            return true;
        }
    }
}
