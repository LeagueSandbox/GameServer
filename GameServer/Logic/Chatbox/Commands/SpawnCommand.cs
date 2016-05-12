using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatboxManager;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    class SpawnCommand : ChatCommand
    {
        public SpawnCommand(string command, string syntax, ChatboxManager owner) : base(command, syntax, owner) { }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            Logger.LogCoreInfo(".spawn command not implemented");
            _owner.SendDebugMsgFormatted(DebugMsgType.INFO, "Command not implemented");
        }
    }
}
