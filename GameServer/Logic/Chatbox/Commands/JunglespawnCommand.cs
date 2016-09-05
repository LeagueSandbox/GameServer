using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using Ninject;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatboxManager;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    class JunglespawnCommand : ChatCommand
    {
        public JunglespawnCommand(string command, string syntax, ChatboxManager owner) : base(command, syntax, owner) { }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            Logger _logger = Program.ResolveDependency<Logger>();
            _logger.LogCoreInfo(".junglespawn command not implemented");
            _owner.SendDebugMsgFormatted(DebugMsgType.INFO, "Command not implemented");
        }
    }
}
