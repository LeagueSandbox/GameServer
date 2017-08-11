using ENet;
using LeagueSandbox.GameServer.Logic.Players;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatCommandManager;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class SpeedCommand : ChatCommandBase
    {
        private readonly PlayerManager _playerManager;

        public override string Command => "speed";
        public override string Syntax => $"{Command} speed";

        public SpeedCommand(ChatCommandManager chatCommandManager, PlayerManager playerManager) : base(chatCommandManager)
        {
            _playerManager = playerManager;
        }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            float speed;
            if (split.Length < 2)
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
            }
            if (float.TryParse(split[1], out speed))
                _playerManager.GetPeerInfo(peer).Champion.GetStats().MoveSpeed.FlatBonus = speed;
            else
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.ERROR, "Incorrect parameter");
        }
    }
}
