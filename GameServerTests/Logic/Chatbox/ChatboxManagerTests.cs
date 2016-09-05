using Microsoft.VisualStudio.TestTools.UnitTesting;
using LeagueSandbox.GameServer.Logic.Chatbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatboxManager;
using LeagueSandbox.GameServer.Logic.Chatbox.Commands;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Content;

namespace GameServerTests
{
    [TestClass()]
    public class ChatboxManagerTests
    {
        [TestMethod()]
        public void AddCommandTest()
        {
            var chatboxManager = new ChatboxManager();

            var command = new HelpCommand("ChatboxManagerTestsTestCommand", "", chatboxManager);
            var result = chatboxManager.AddCommand(command);
            Assert.AreEqual(true, result);
            result = chatboxManager.AddCommand(command);
            Assert.AreEqual(false, result);
        }

        [TestMethod()]
        public void RemoveCommandTest()
        {
            var chatboxManager = new ChatboxManager();

            var command = new HelpCommand("ChatboxManagerTestsTestCommand", "", chatboxManager);
            var result = chatboxManager.AddCommand(command);
            Assert.AreEqual(true, result);

            var result2 = chatboxManager.RemoveCommand("ChatboxManagerTestsTestCommand");
            var commands = chatboxManager.GetCommandsStrings();
            Assert.AreEqual(true, result2);
            if (commands.Contains("ChatboxManagerTestsTestCommand"))
            {
                Assert.Fail();
            }
        }
    }
}