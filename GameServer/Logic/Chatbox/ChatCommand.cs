using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Chatbox
{
    public abstract class ChatCommand
    {
        internal ChatboxManager _owner;

        public string Command { get; set; }
        public string Syntax { get; set; }
        public bool IsHidden { get; set; }
        public bool IsDisabled { get; set; }

        public ChatCommand(string command, string syntax, ChatboxManager owner)
        {
            Command = command;
            Syntax = syntax;
            _owner = owner;
        }

        public abstract void Execute(Peer peer, bool hasReceivedArguments, string arguments = "");

        public void ShowSyntax()
        {
            _owner.SendDebugMsgFormatted(ChatboxManager.DebugMsgType.SYNTAX, _owner.CommandStarterCharacter + Syntax);
        }
    }
}
