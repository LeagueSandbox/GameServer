using ENet;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatboxManager;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    class SetCommand : ChatCommand
    {
        public SetCommand(string command, string syntax, ChatboxManager owner) : base(command, syntax, owner) { }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            if (split.Length < 4)
            {
                _owner.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
                return;
            }
            int blockNo, fieldNo = 0;
            float value = 0;
            if (int.TryParse(split[1], out blockNo))
                if (int.TryParse(split[2], out fieldNo))
                    if (float.TryParse(split[3], out value))
                    {
                        //game.GetPeerInfo(peer).Champion.GetStats().setStat((MasterMask)blockNo, (FieldMask)fieldNo, value);
                    }
        }
    }
}
