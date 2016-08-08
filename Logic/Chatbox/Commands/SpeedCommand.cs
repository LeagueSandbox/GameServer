﻿using ENet;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatboxManager;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    class SpeedCommand : ChatCommand
    {
        public SpeedCommand(string command, string syntax, ChatboxManager owner) : base(command, syntax, owner) { }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            float speed;
            if (split.Length < 2)
            {
                _owner.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
            }
            if (float.TryParse(split[1], out speed))
                _owner.GetGame().GetPeerInfo(peer).GetChampion().GetStats().MoveSpeed.FlatBonus = speed;
            else
                _owner.SendDebugMsgFormatted(DebugMsgType.ERROR, "Incorrect parameter");
        }
    }
}
