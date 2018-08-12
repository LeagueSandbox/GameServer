using System;
using ENet;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Players;
using Packet = LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.Packet;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class PacketCommand : ChatCommandBase
    {
        private readonly PlayerManager _playerManager;

        public override string Command => "packet";
        public override string Syntax => $"{Command} XX XX XX...";

        public PacketCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _playerManager = game.PlayerManager;
        }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            try
            {
                var s = arguments.Split(' ');
                if (s.Length < 2)
                {
                    ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                    ShowSyntax();
                    return;
                }

                var opcode = Convert.ToByte(s[1], 16);
                var packet = new Packet((PacketCmd)opcode);

                for (var i = 2; i < s.Length; i++)
                {
                    if (s[i].Equals("netid"))
                    {
                        packet.Write(_playerManager.GetPeerInfo(peer).Champion.NetId);
                    }
                    else
                    {
                        packet.Write(Convert.ToByte(s[i], 16));
                    }
                }

                Game.PacketHandlerManager.SendPacket(peer, packet, Channel.CHL_S2C);
            }
            catch
            {
                // ignored
            }
        }
    }
}
