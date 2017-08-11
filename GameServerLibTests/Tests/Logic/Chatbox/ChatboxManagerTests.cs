using Microsoft.VisualStudio.TestTools.UnitTesting;
using LeagueSandbox.GameServer.Logic.Chatbox;
using LeagueSandbox.GameServer.Logic.Chatbox.Commands;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServerTests.Tests.Logic.Chatbox;

namespace LeagueSandbox.GameServerTests.Tests
{
    [TestClass]
    public class ChatboxManagerTests
    {
        [TestMethod]
        public void AddCommandTest()
        {
            var chatboxManager = new ChatCommandManager(new PacketHandlerProvider());
            var command = new TestCommand(chatboxManager, "ChatboxManagerTestsTestCommand", "");
            var result = chatboxManager.AddCommand(command);
            Assert.AreEqual(true, result);
            result = chatboxManager.AddCommand(command);
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void RemoveCommandTest()
        {
            var chatboxManager = new ChatCommandManager(new PacketHandlerProvider());

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
        }
    }
}