using ENet;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class CoordsCommand : ChatCommandBase
    {

        public override string Command => "coords";
        public override string Syntax => $"{Command}";

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var champion = PlayerManager.GetPeerInfo(peer).Champion;
            Logger.LogCoreInfo($"At {champion.X}; {champion.Y}");
            var msg = $"At Coords - X: {champion.X} Y: {champion.Y} Z: {champion.GetZ()}";
            ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.NORMAL, msg);
        }
    }
}
