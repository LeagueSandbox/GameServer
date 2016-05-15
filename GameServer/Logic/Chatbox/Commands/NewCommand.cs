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
            var test = new Packets.Packet((PacketCmdS2C)0xF9);

            var buffer = test.getBuffer();
            buffer.Write(0x00);
            buffer.Write(0xFF);

            _owner.GetGame().PacketHandlerManager.sendPacket(peer, test, Channel.CHL_S2C);



            /*var floatingText = "Hello guys!";//"game_lua_Highlander");
            var buffer2 = packet2.getBuffer();
            buffer2.Write(netid);
            buffer2.fill(0, 10);
            buffer2.Write(netid);
            foreach (var b in Encoding.Default.GetBytes(floatingText))
                buffer2.Write(b);

            _owner.GetGame().PacketHandlerManager.sendPacket(peer, packet2, Channel.CHL_S2C);*/

            _owner.SendDebugMsgFormatted(DebugMsgType.INFO, "The new command added by " + _owner.CommandStarterCharacter + "help has been executed");
            //_owner.RemoveCommand(Command);
        }
    }
}
