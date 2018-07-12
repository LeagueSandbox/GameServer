using ENet;
using LeagueSandbox.GameServer.Logic.Chatbox;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleBlueTipClicked : PacketHandlerBase
    {
        public override PacketCmd PacketType => PacketCmd.PKT_C2S_BLUE_TIP_CLICKED;
        public override Channel PacketChannel => Channel.CHL_C2_S;

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var blueTipClicked = new BlueTipClicked(data);
            var removeBlueTip = new BlueTip(
                "",
                "",
                "",
                0,
                PlayerManager.GetPeerInfo(peer).Champion.NetId,
                blueTipClicked.Netid);

            Game.PacketHandlerManager.SendPacket(peer, removeBlueTip, Channel.CHL_S2_C);
            var msg = $"Clicked blue tip with netid: {blueTipClicked.Netid}";
            ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.NORMAL, msg);
            return true;
        }
    }
}
