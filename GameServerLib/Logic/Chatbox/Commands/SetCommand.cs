using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatCommandManager;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class SetCommand : ChatCommandBase
    {
        private readonly Game _game;

        public override string Command => "set";
        public override string Syntax => $"{Command} masterMask fieldMask";

        public SetCommand(ChatCommandManager chatCommandManager, Game game) : base(chatCommandManager)
        {
            _game = game;
        }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            if (split.Length < 4)
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
                return;
            }
            int blockNo, fieldNo = 0;
            float value = 0;
            if (int.TryParse(split[1], out blockNo))
                if (int.TryParse(split[2], out fieldNo))
                    if (float.TryParse(split[3], out value))
                    {
                        //_game.GetPeerInfo(peer).Champion.GetStats().setStat((MasterMask)blockNo, (FieldMask)fieldNo, value);
                    }
        }
    }
}
