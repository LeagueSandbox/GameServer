using Microsoft.VisualStudio.TestTools.UnitTesting;
using LeagueSandbox.GameServerApp;

namespace LeagueSandbox.GameServerAppTests
{
    [TestClass]
    public class TestArgsOptions
    {
        [TestMethod]
        public void TestConfigAndPort()
        {
            var port = 5839;
            var config = "/some/config/path.json";
            var args = new string[]
            {
                "--port", port.ToString(),
                "--config", config
            };
            var options = ArgsOptions.Parse(args);
            Assert.AreEqual(port, options.ServerPort);
            Assert.AreEqual(config, options.ConfigPath);
        }

        [TestMethod]
        public void TestDefaults()
        {
            var options = ArgsOptions.Parse(new string[0]);
            Assert.IsTrue(options.ServerPort > 0);
            Assert.IsFalse(string.IsNullOrEmpty(options.ConfigPath));
        }
    }
}
