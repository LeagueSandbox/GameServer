using Microsoft.VisualStudio.TestTools.UnitTesting;
using LeagueSandbox.GameServer.Logic.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServerTests;

namespace LeagueSandbox.GameServer.Logic.GameObjects.Tests
{
    [TestClass()]
    public class BuffManagerTests
    {
        [TestMethod()]
        public void RemoveBuffTest()
        {
            var game = new TestHelpers.DummyGame();
            game.LoadBuffManager();

            var result = game.BuffManager.RemoveBuff("Haste");
            Assert.IsFalse(result);
        }
    }
}