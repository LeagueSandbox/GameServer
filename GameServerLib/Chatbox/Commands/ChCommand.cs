using ENet;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Players;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class ChCommand : ChatCommandBase
    {
        private readonly PlayerManager _playerManager;

        public override string Command => "ch";
        public override string Syntax => $"{Command} championName";

        public ChCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _playerManager = game.PlayerManager;
        }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.Split(' ');
            if (split.Length < 2)
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
                return;
            }
            var currentChampion = _playerManager.GetPeerInfo(peer).Champion;

            var c = new Champion(
                Game,
                split[1],
                (uint)_playerManager.GetPeerInfo(peer).UserId,
                0, // Doesnt matter at this point
                (RuneCollection)currentChampion.RuneList,
                _playerManager.GetClientInfoByChampion((Champion)currentChampion),
                currentChampion.NetId
            );
            c.SetPosition(
                _playerManager.GetPeerInfo(peer).Champion.X,
                _playerManager.GetPeerInfo(peer).Champion.Y
            );

            c.Model = split[1]; // trigger the "modelUpdate" proc
            c.SetTeam(_playerManager.GetPeerInfo(peer).Champion.Team);
            Game.ObjectManager.RemoveObject((Champion)_playerManager.GetPeerInfo(peer).Champion);
            Game.ObjectManager.AddObject(c);
            _playerManager.GetPeerInfo(peer).Champion = c;
        }
    }
}
