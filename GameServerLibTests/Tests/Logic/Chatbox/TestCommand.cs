using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using LeagueSandbox.GameServer.Logic.Chatbox;

namespace LeagueSandbox.GameServerTests.Tests.Logic.Chatbox
{
    public class TestCommand : ChatCommandBase
    {
        public override string Command { get; }
        public override string Syntax { get; }

        public TestCommand(ChatCommandManager chatCommandManager, string command, string syntax) : base(chatCommandManager)
        {
            Command = command;
            Syntax = syntax;
        }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {

        }
    }
}
