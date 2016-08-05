﻿using ENet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatboxManager;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    class AdCommand : ChatCommand
    {
        public AdCommand(string command, string syntax, ChatboxManager owner) : base(command, syntax, owner) { }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            float ad;
            if (split.Length < 2)
            {
                _owner.SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ShowSyntax();
            }
            else if (float.TryParse(split[1], out ad))
                _owner.GetGame().GetPeerInfo(peer).GetChampion().GetStats().AttackDamage.FlatBonus = ad;
        }
    }
}
