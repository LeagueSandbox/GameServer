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

            /*var coso = _owner.GetGame().GimmeCoso();
            _owner.SendDebugMsgFormatted(DebugMsgType.INFO, "Coso: " + coso);
            Console.WriteLine(coso.ToString());*/
            if (true)//!Enum.IsDefined(typeof(ExtendedPacketCmd), (uint)coso))
            {
                var packet = new Packets.ExtendedPacket((ExtendedPacketCmd)(byte)0x13, netid);

                var buffer = packet.getBuffer();
                buffer.Write(0x07);

                _owner.GetGame().PacketHandlerManager.sendPacket(peer, packet, Channel.CHL_S2C);
                _owner.SendDebugMsgFormatted(DebugMsgType.INFO, "Sent :D");
            }

            var packet2 = new Packets.BasePacket((PacketCmdS2C)0xB2, netid);
            /*var buffer2 = packet2.getBuffer();
            buffer2.Write(0x02);
            buffer2.Write(0x05);
            buffer2.Write(0x07);*/

            _owner.GetGame().PacketHandlerManager.sendPacket(peer, packet2, Channel.CHL_S2C);


            _owner.SendDebugMsgFormatted(DebugMsgType.INFO, "The new command added by " + _owner.CommandStarterCharacter + "help has been executed");
            //_owner.RemoveCommand(Command);
        }
    }
}
