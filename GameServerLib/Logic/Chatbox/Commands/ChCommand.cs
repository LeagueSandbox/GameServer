using ENet;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    public class ChCommand : ChatCommandBase
    {
        public override string Command => "ch";
        public override string Syntax => $"{Command} championName";
        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.Split(' ');
            if (split.Length < 2)
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
                return;
            }
            var currentChampion = PlayerManager.GetPeerInfo(peer).Champion;

            var c = new Champion(
                split[1],
                (uint)PlayerManager.GetPeerInfo(peer).UserId,
                0, // Doesnt matter at this point
                currentChampion.RuneList,
                PlayerManager.GetClientInfoByChampion(currentChampion),
                currentChampion.NetId
            );
            c.SetPosition(
                PlayerManager.GetPeerInfo(peer).Champion.X,
                PlayerManager.GetPeerInfo(peer).Champion.Y
            );

            c.Model = split[1]; // trigger the "modelUpdate" proc
            c.SetTeam(PlayerManager.GetPeerInfo(peer).Champion.Team);
            Game.ObjectManager.RemoveObject(PlayerManager.GetPeerInfo(peer).Champion);
            Game.ObjectManager.AddObject(c);
            PlayerManager.GetPeerInfo(peer).Champion = c;
        }
    }
}
