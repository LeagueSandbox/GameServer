using LeagueSandbox.GameServer.Logic.Chatbox;
using LeagueSandbox.GameServerTests.Tests.Logic.Chatbox;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LeagueSandbox.GameServerTests.Tests
{
    [TestClass]
    public class ChatboxManagerTests
    {
        [TestMethod]
        public void AddCommandTest()
        {
            /*
            var chatboxManager = _kernel.Get<ChatCommandManager>();
            var command = new TestCommand(chatboxManager, "ChatboxManagerTestsTestCommand", "");
            var result = chatboxManager.AddCommand(command);
            Assert.AreEqual(true, result);
            result = chatboxManager.AddCommand(command);
            Assert.AreEqual(false, result);
            */
        }

        [TestMethod]
        public void RemoveCommandTest()
        {
            /*
            var chatboxManager = _kernel.Get<ChatCommandManager>();

            var command = new TestCommand(chatboxManager, "ChatboxManagerTestsTestCommand", "");
            var result = chatboxManager.AddCommand(command);
            Assert.AreEqual(true, result);

            var result2 = chatboxManager.RemoveCommand("ChatboxManagerTestsTestCommand");
            var commands = chatboxManager.GetCommandsStrings();
            Assert.AreEqual(true, result2);
            if (commands.Contains("ChatboxManagerTestsTestCommand"))
            {
                Assert.Fail();
            }
            */
        }
    }
}