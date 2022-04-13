using LeagueSandbox.GameServer;
using LeagueSandbox.GameServer.Chatbox;
using LeagueSandbox.GameServer.Inventory;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LeagueSandbox.GameServerTests.Tests.Chatbox
{
    [TestClass]
    public class ChatboxManagerTests
    {
        private readonly Game _game = new Game();
        [TestMethod]
        public void AddCommandTest()
        {
            var chatboxManager = new ChatCommandManager(_game);
            var command = new TestCommand(_game, chatboxManager, "ChatboxManagerTestsTestCommand", "");
            var result = chatboxManager.AddCommand(command);
            Assert.AreEqual(true, result);
            result = chatboxManager.AddCommand(command);
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void RemoveCommandTest()
        {
            var chatboxManager = new ChatCommandManager(_game);
            var command = new TestCommand(_game, chatboxManager, "ChatboxManagerTestsTestCommand", "");
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
