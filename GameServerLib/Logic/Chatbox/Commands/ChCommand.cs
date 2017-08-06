using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Players;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatCommandManager;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    class ChCommand : ChatCommand
    {

        public ChCommand(string command, string syntax, ChatCommandManager owner) : base(command, syntax, owner) { }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            Game _game = Program.ResolveDependency<Game>();
            PlayerManager _playerManager = Program.ResolveDependency<PlayerManager>();

            var split = arguments.Split(' ');
            if (split.Length < 2)
            {
                _owner.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
                return;
            }
            var currentChampion = _playerManager.GetPeerInfo(peer).Champion;
            var c = new Champion(
                split[1],
                (uint)_playerManager.GetPeerInfo(peer).UserId,
                0, // Doesnt matter at this point
                currentChampion.RuneList,
                _playerManager.GetClientInfoByChampion(currentChampion),
                currentChampion.NetId
            );
            c.setPosition(
                _playerManager.GetPeerInfo(peer).Champion.X,
                _playerManager.GetPeerInfo(peer).Champion.Y
            );
            c.Model = split[1]; // trigger the "modelUpdate" proc
            c.SetTeam(_playerManager.GetPeerInfo(peer).Champion.Team);
            _game.ObjectManager.RemoveObject(_playerManager.GetPeerInfo(peer).Champion);
            _game.ObjectManager.AddObject(c);
            _playerManager.GetPeerInfo(peer).Champion = c;
        }
    }
}
