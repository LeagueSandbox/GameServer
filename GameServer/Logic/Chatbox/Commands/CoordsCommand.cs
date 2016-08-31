using ENet;
using LeagueSandbox.GameServer.Core.Logic;
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
            Logger _logger = Program.Kernel.Get<Logger>();
            _logger.LogCoreInfo("At " + _owner.GetGame().GetPeerInfo(peer).GetChampion().getX() + ";" + _owner.GetGame().GetPeerInfo(peer).GetChampion().getY());
            StringBuilder debugMsg = new StringBuilder();
            debugMsg.Append("At Coords - X: ");
            debugMsg.Append(_owner.GetGame().GetPeerInfo(peer).GetChampion().getX());
            debugMsg.Append(" Y: ");
            debugMsg.Append(_owner.GetGame().GetPeerInfo(peer).GetChampion().getY());
            debugMsg.Append(" Z: ");
            debugMsg.Append(_owner.GetGame().GetPeerInfo(peer).GetChampion().GetZ());
            _owner.SendDebugMsgFormatted(DebugMsgType.NORMAL, debugMsg.ToString());
        }
    }
}
