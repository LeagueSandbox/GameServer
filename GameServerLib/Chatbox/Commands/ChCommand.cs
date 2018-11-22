using GameServerCore;
using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;

namespace LeagueSandbox.GameServer.Chatbox.Commands
{
    public class ChCommand : ChatCommandBase
    {
        private readonly IPlayerManager _playerManager;

        public override string Command => "ch";
        public override string Syntax => $"{Command} championName";

        public ChCommand(ChatCommandManager chatCommandManager, Game game)
            : base(chatCommandManager, game)
        {
            _playerManager = game.PlayerManager;
        }

        public override void Execute(int userId, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.Split(' ');
            if (split.Length < 2)
            {
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
                return;
            }
            var currentChampion = _playerManager.GetPeerInfo(userId).Champion;

            var c = new Champion(
                Game,
                split[1],
                (uint)_playerManager.GetPeerInfo(userId).UserId,
                0, // Doesnt matter at this point
                currentChampion.RuneList,
                _playerManager.GetClientInfoByChampion(currentChampion),
                currentChampion.NetId
            );
            c.SetPosition(
                _playerManager.GetPeerInfo(userId).Champion.X,
                _playerManager.GetPeerInfo(userId).Champion.Y
            );

            c.ChangeModel(split[1]); // trigger the "modelUpdate" proc
            c.SetTeam(_playerManager.GetPeerInfo(userId).Champion.Team);
            Game.ObjectManager.RemoveObject(_playerManager.GetPeerInfo(userId).Champion);
            Game.ObjectManager.AddObject(c);
            _playerManager.GetPeerInfo(userId).Champion = c;
        }
    }
}
