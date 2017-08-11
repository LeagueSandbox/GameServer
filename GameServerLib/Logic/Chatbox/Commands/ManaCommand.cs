using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Players;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatCommandManager;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class ManaCommand : ChatCommandBase
    {
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override string Command => "mana";
        public override string Syntax => $"{Command} maxMana";

        public ManaCommand(ChatCommandManager chatCommandManager, Game game, PlayerManager playerManager) : base(chatCommandManager)
        {
            _game = game;
            _playerManager = playerManager;
        }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            float mp;
            if (split.Length < 2)
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
            }
            else if (float.TryParse(split[1], out mp))
            {
                _playerManager.GetPeerInfo(peer).Champion.GetStats().ManaPoints.FlatBonus = mp;
                _playerManager.GetPeerInfo(peer).Champion.GetStats().CurrentMana = mp;
            }
        }
    }
}
