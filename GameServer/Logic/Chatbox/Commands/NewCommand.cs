using ENet;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatboxManager;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    class NewCommand : ChatCommand
    {
        public NewCommand(string command, string syntax, ChatboxManager owner) : base(command, syntax, owner) { }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var netid = _owner.GetGame().GetPeerInfo(peer).GetChampion().getNetId();
            var packet = new Packets.BasePacket((PacketCmdS2C)0x18, netid);
            var buffer = packet.getBuffer();

            /*buffer.Write(" game_startup_tip_1_nautilus");
            buffer.fill(0, 102);
            buffer.Write(" Hola");
            buffer.fill(0, 230);
            buffer.Write((byte)0x01);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);*/

            buffer.Write(netid);
            buffer.fill(0, 10);
            buffer.Write(netid);
            buffer.Write("Hello again");//"game_lua_Highlander");

            _owner.GetGame().PacketHandlerManager.sendPacket(peer, packet, Channel.CHL_S2C);
            _owner.SendDebugMsgFormatted(DebugMsgType.INFO, "The new command added by " + _owner.CommandStarterCharacter + "help has been executed");
            //_owner.RemoveCommand(Command);
        }
    }
}
