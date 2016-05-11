using ENet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatboxManager;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    class NewCommand : ChatCommand
    {
        public NewCommand(string command, string syntax, ChatboxManager owner) : base(command, syntax, owner) { }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            /*var packet = new GameServer.Logic.Packets.Packet((PacketCmdS2C)0xB7);
            var netid = game.GetPeerInfo(peer).GetChampion().getNetId();
            var buffer = packet.getBuffer();

            buffer.Write(netid);//target
            buffer.Write((byte)0x01); //Slot
            buffer.Write((byte)0x01); //Type
            buffer.Write((byte)0x01); // stacks
            buffer.Write((byte)0x01); // Visible
            buffer.Write((int)17212821); //Buff id
            buffer.Write((byte)0x56);
            buffer.Write((byte)0xD0);
            buffer.Write((byte)0xF2);
            buffer.Write((byte)0xDF);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);

            buffer.Write((float)25000.0f);

            buffer.Write((byte)0x20);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x40);
            buffer.Write((int)0);

            game.PacketHandlerManager.sendPacket(peer, packet, Channel.CHL_S2C);*/
            _owner.SendDebugMsgFormatted(DebugMsgType.INFO, "The new command added by .help has been executed");
            //_owner.RemoveCommand(Command);
        }
    }
}
