using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                _playerManager.GetPeerInfo(peer).GetChampion().runeList
            );
            c.setPosition(
                _playerManager.GetPeerInfo(peer).GetChampion().getX(),
                _playerManager.GetPeerInfo(peer).GetChampion().getY()
            );
            c.setModel(split[1]); // trigger the "modelUpdate" proc
            c.setTeam(_playerManager.GetPeerInfo(peer).GetChampion().getTeam());
            _game.GetMap().RemoveObject(_playerManager.GetPeerInfo(peer).GetChampion());
            _game.GetMap().AddObject(c);
            _playerManager.GetPeerInfo(peer).SetChampion(c);
        }
    }
}
