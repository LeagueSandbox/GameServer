using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Players;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatboxManager;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    class CoordsCommand : ChatCommand
    {
        public CoordsCommand(string command, string syntax, ChatboxManager owner) : base(command, syntax, owner) { }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            Logger _logger = Program.ResolveDependency<Logger>();
            PlayerManager _playerManager = Program.ResolveDependency<PlayerManager>();

            _logger.LogCoreInfo("At " + _playerManager.GetPeerInfo(peer).GetChampion().getX() + ";" + _playerManager.GetPeerInfo(peer).GetChampion().getY());
            StringBuilder debugMsg = new StringBuilder();
            debugMsg.Append("At Coords - X: ");
            debugMsg.Append(_playerManager.GetPeerInfo(peer).GetChampion().getX());
            debugMsg.Append(" Y: ");
            debugMsg.Append(_playerManager.GetPeerInfo(peer).GetChampion().getY());
            debugMsg.Append(" Z: ");
            debugMsg.Append(_playerManager.GetPeerInfo(peer).GetChampion().GetZ());
            _owner.SendDebugMsgFormatted(DebugMsgType.NORMAL, debugMsg.ToString());
        }
    }
}
