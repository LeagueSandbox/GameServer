using System;
using ENet;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Players;
using Packet = LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.Packet;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class PacketCommand : ChatCommandBase
    {
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override string Command => "packet";
        public override string Syntax => $"{Command} XX XX XX...";

        public PacketCommand(ChatCommandManager chatCommandManager, Game game, PlayerManager playerManager)
            : base(chatCommandManager)
        {
            _game = game;
            _playerManager = playerManager;
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

                _game.PacketHandlerManager.SendPacket(peer, packet, Channel.CHL_S2_C);
            }
            catch
            {
                // ignored
            }
        }
    }
}
