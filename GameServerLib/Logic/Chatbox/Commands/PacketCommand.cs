using System;
using ENet;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatCommandManager;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class PacketCommand : ChatCommandBase
    {
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override string Command => "packet";
        public override string Syntax => $"{Command} XX XX XX...";

        public PacketCommand(ChatCommandManager chatCommandManager, Game game, PlayerManager playerManager) : base(chatCommandManager)
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
                var packet = new Packets.Packet((PacketCmd)opcode);
                var buffer = packet.getBuffer();

                for (int i = 2; i < s.Length; i++)
                {
                    if (s[i] == "netid")
                    {
                        buffer.Write(_playerManager.GetPeerInfo(peer).Champion.NetId);
                    }
                    else
                    {
                        buffer.Write(Convert.ToByte(s[i], 16));
                    }
                }

                _game.PacketHandlerManager.sendPacket(peer, packet, Channel.CHL_S2C);
            }
            catch { }
        }
    }
}
