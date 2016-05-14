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
            var packet2 = new Packets.BasePacket((PacketCmdS2C)PacketCmdS2C.PKT_S2C_FloatingText);

            var newnetid = _owner.GetGame().GetNewNetID();
            var newnetid2 = _owner.GetGame().GetNewNetID();
            var TipPacket = new Packets.BlueTip("Blue tip title", "Netid: " + newnetid, netid, newnetid);
            var TipPacket2 = new Packets.BlueTip("Blue tip title", "Netid: " + newnetid2, netid, newnetid2);

            var QuestPacket = new Packets.Quest("Title", "<subtitleRight>Subtitle</subtitleRight><br><br><maintext>Maintext</maintext>", 2, 0, _owner.GetGame().GetNewNetID());
            _owner.GetGame().PacketHandlerManager.sendPacket(peer, QuestPacket, Channel.CHL_S2C);

            var floatingText = "Hello guys!";//"game_lua_Highlander");
            var buffer2 = packet2.getBuffer();
            buffer2.Write(netid);
            buffer2.fill(0, 10);
            buffer2.Write(netid);
            foreach (var b in Encoding.Default.GetBytes(floatingText))
                buffer2.Write(b);

            _owner.GetGame().PacketHandlerManager.sendPacket(peer, TipPacket2, Channel.CHL_S2C);
            _owner.GetGame().PacketHandlerManager.sendPacket(peer, TipPacket, Channel.CHL_S2C);
            //_owner.GetGame().PacketHandlerManager.sendPacket(peer, packet2, Channel.CHL_S2C);

            _owner.SendDebugMsgFormatted(DebugMsgType.INFO, "The new command added by " + _owner.CommandStarterCharacter + "help has been executed");
            //_owner.RemoveCommand(Command);
        }
    }
}
