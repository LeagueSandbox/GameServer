using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Players;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatCommandManager;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    class TpCommand : ChatCommand
    {
        public TpCommand(string command, string syntax, ChatCommandManager owner) : base(command, syntax, owner) { }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            Game _game = Program.ResolveDependency<Game>();
            PlayerManager _playerManager = Program.ResolveDependency<PlayerManager>();

            var split = arguments.ToLower().Split(' ');
            float x, y;
            if (split.Length < 3)
            {
                _owner.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
                return;
            }
            if (float.TryParse(split[1], out x))
                if (float.TryParse(split[2], out y))
                    _game.PacketNotifier.NotifyTeleport(_playerManager.GetPeerInfo(peer).Champion, x, y);
        }
    }
}
