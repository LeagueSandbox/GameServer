using ENet;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class JunglespawnCommand : ChatCommandBase
    {
        public override string Command => "junglespawn";
        public override string Syntax => $"{Command}";
        

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            Logger.LogCoreInfo($"{ChatCommandManager.CommandStarterCharacter}{Command} command not implemented");
            ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.INFO, "Command not implemented");
        }
    }
}
