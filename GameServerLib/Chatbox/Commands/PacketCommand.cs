using GameServerCore;
using System;
using System.Collections.Generic;

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
                List<byte> _bytes = new List<byte>();
                _bytes.Add(Convert.ToByte(s[1], 16));

                for (var i = 2; i < s.Length; i++)
                {
                    if (s[i].Equals("netid"))
                    {
                        _bytes.Add(Convert.ToByte(_playerManager.GetPeerInfo(userId).Champion.NetId));
                    }
                    else
                    {
                        _bytes.Add(Convert.ToByte(s[i], 16));
                    }
                }

                _game.PacketNotifier.NotifyDebugPacket(userId, _bytes.ToArray());
            }
            catch
            {
                // ignored
            }
        }
    }
}
