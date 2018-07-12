﻿using ENet;
using LeagueSandbox.GameServer.Logic.Chatbox;

namespace LeagueSandbox.GameServerTests.Tests.Logic.Chatbox
{
    public class TestCommand : ChatCommandBase
    {
        public override string Command { get; }
        public override string Syntax { get; }

        public TestCommand(string command, string syntax)
        {
            Command = command;
            Syntax = syntax;
        }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {

        }
    }
}
