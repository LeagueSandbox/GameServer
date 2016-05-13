using ENet;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using System;
using System.Text;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatboxManager;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    class NewCommand : ChatCommand
    {
        public NewCommand(string command, string syntax, ChatboxManager owner) : base(command, syntax, owner) { }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var netid = _owner.GetGame().GetPeerInfo(peer).GetChampion().getNetId();
            //var packet = new Packets.BasePacket((PacketCmdS2C)PacketCmdS2C.PKT_S2C_BlueTip, netid);
            var packet2 = new Packets.BasePacket((PacketCmdS2C)PacketCmdS2C.PKT_S2C_FloatingText, netid);
            //var buffer = packet.getBuffer();

            var text = "Hello boys!"; //"game_aram_tip_text_noheal";
            var title = "Custom text!, yay"; //"game_aram_tip_title_noheal";
            /*foreach (var b in Encoding.Default.GetBytes(text))
                buffer.Write(b);
            buffer.fill(0, 128-text.Length);
            foreach (var b in Encoding.Default.GetBytes(title))
                buffer.Write(b);
            buffer.fill(0, 256-title.Length);
            buffer.Write((byte)0x02);
            buffer.Write((byte)0x0B);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x01);
            buffer.Write((byte)0x01);*/

            //Console.WriteLine(buffer.BaseStream.Length.ToString());

            var TipPacket = new Packets.BlueTip("Blue tip", "slot 0", netid, 0);
            var TipPacket2 = new Packets.BlueTip("Blue tip", "slot 1", netid, 1);


            var floatingText = "Hello guys!";//"game_lua_Highlander");
            var buffer2 = packet2.getBuffer();
            buffer2.Write(netid);
            buffer2.fill(0, 10);
            buffer2.Write(netid);
            foreach (var b in Encoding.Default.GetBytes(floatingText))
                buffer2.Write(b);

            _owner.GetGame().PacketHandlerManager.sendPacket(peer, TipPacket, Channel.CHL_S2C);
            _owner.GetGame().PacketHandlerManager.sendPacket(peer, TipPacket2, Channel.CHL_S2C);
            _owner.GetGame().PacketHandlerManager.sendPacket(peer, packet2, Channel.CHL_S2C);
            _owner.SendDebugMsgFormatted(DebugMsgType.INFO, "The new command added by " + _owner.CommandStarterCharacter + "help has been executed");
            //_owner.RemoveCommand(Command);
        }
    }
}
