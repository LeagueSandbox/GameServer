using ENet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatboxManager;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    class TpCommand : ChatCommand
    {
        public TpCommand(string command, string syntax, ChatboxManager owner) : base(command, syntax, owner) { }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
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
                    _owner.GetGame().PacketNotifier.notifyTeleport(_owner.GetGame().GetPeerInfo(peer).GetChampion(), x, y);
        }
    }
}
