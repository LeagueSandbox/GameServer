using ENet;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    class ReviveCommand : ChatCommand
    {
        public ReviveCommand(string command, string syntax, ChatCommandManager owner) : base(command, syntax, owner)
        {
        }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var playerManager = Program.ResolveDependency<PlayerManager>();
            var champ = playerManager.GetPeerInfo(peer).Champion;
            if (!champ.IsDead)
            {
                _owner.SendDebugMsgFormatted(ChatCommandManager.DebugMsgType.INFO, "Your champion is already alive.");
                return;
            }
            _owner.SendDebugMsgFormatted(ChatCommandManager.DebugMsgType.INFO, "Your champion has revived!");
            champ.Respawn();
        }
    }
}
