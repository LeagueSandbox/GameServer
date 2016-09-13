using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Players;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatboxManager;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    class ChCommand : ChatCommand
    {

        public ChCommand(string command, string syntax, ChatboxManager owner) : base(command, syntax, owner) { }

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
            var c = new Champion(
                split[1],
                (uint)_playerManager.GetPeerInfo(peer).UserId,
                _playerManager.GetPeerInfo(peer).Champion.RuneList,
                _playerManager.GetPeerInfo(peer).Champion.NetId
            );
            c.setPosition(
                _playerManager.GetPeerInfo(peer).Champion.getX(),
                _playerManager.GetPeerInfo(peer).Champion.getY()
            );
            c.Model = split[1]; // trigger the "modelUpdate" proc
            c.Team = _playerManager.GetPeerInfo(peer).Champion.Team;
            _game.GetMap().RemoveObject(_playerManager.GetPeerInfo(peer).Champion);
            _game.GetMap().AddObject(c);
            _playerManager.GetPeerInfo(peer).Champion = c;
        }
    }
}
