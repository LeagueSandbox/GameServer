using ENet;
using LeagueSandbox.GameServer.Logic.GameObjects;
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
            var split = arguments.ToLower().Split(' ');
            if (split.Length < 2)
            {
                _owner.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
                return;
            }
            new System.Threading.Thread(new System.Threading.ThreadStart(() =>
            {
                var c = new Champion(_owner.GetGame(), split[1], _owner.GetGame().GetPeerInfo(peer).GetChampion().getNetId(), (uint)_owner.GetGame().GetPeerInfo(peer).UserId);
                c.setPosition(_owner.GetGame().GetPeerInfo(peer).GetChampion().getX(), _owner.GetGame().GetPeerInfo(peer).GetChampion().getY());
                c.setModel(split[1]); // trigger the "modelUpdate" proc
                c.setTeam(_owner.GetGame().GetPeerInfo(peer).GetChampion().getTeam());
                _owner.GetGame().GetMap().RemoveObject(_owner.GetGame().GetPeerInfo(peer).GetChampion());
                _owner.GetGame().GetMap().AddObject(c);
                _owner.GetGame().GetPeerInfo(peer).SetChampion(c);
            })).Start();
        }
    }
}
