using Microsoft.VisualStudio.TestTools.UnitTesting;
using LeagueSandbox.GameServer.Logic.Chatbox;
using LeagueSandbox.GameServer.Logic.Chatbox.Commands;

namespace GameServerTests.Tests
{
    [TestClass]
    public class ChatboxManagerTests
    {
        [TestMethod]
        public void AddCommandTest()
        {
            var chatboxManager = new ChatCommandManager();

            var command = new HelpCommand("ChatboxManagerTestsTestCommand", "", chatboxManager);
            var result = chatboxManager.AddCommand(command);
            Assert.AreEqual(true, result);
            result = chatboxManager.AddCommand(command);
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void RemoveCommandTest()
        {
            var chatboxManager = new ChatCommandManager();

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