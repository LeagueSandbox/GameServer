using ENet;
using LeagueSandbox.GameServer.Logic.Players;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatCommandManager;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class XpCommand : ChatCommandBase
    {
        private readonly PlayerManager _playerManager;

        public override string Command => "xp";
        public override string Syntax => $"{Command} xp";

        public XpCommand(ChatCommandManager chatCommandManager, PlayerManager playerManager) : base(chatCommandManager)
        {
            _playerManager = playerManager;
        }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            float xp;
            if (split.Length < 2)
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
                return;
            }
            if (float.TryParse(split[1], out xp))
                _playerManager.GetPeerInfo(peer).Champion.GetStats().Experience = xp;
        }
    }
}
