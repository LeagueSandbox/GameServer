using GameServerCore;
using GameServerCore.Packets.Enums;
using System;
using Packet = GameServerCore.Packets.PacketDefinitions.Packet;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class PacketCommand : ChatCommandBase
    {
        private readonly IPlayerManager _playerManager;
        private readonly Game _game;

        public override string Command => "packet";
        public override string Syntax => $"{Command} XX XX XX...";

        public PacketCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _playerManager = game.PlayerManager;
            _game = game;
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
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
                        packet.Write(_playerManager.GetPeerInfo(userId).Champion.NetId);
                    }
                    else
                    {
                        packet.Write(Convert.ToByte(s[i], 16));
                    }
                }

                _game.PacketNotifier.NotifyDebugPacket(userId, packet.GetBytes());
            }
            catch
            {
                // ignored
            }
        }
    }
}
