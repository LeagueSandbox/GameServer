using System;
using LeagueSandbox.GameServer.Logic.GameObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameServerTests
{
    [TestClass]
    public class TestStat
    {
        [TestMethod]
        public void TestTotal()
        {
            //Create new Stat object with everything set to 0
            var stat = new Stat();
            //Make sure total is equal to 0
            Assert.AreEqual(0, stat.Total);
            
            //Add 1 to base value
            stat.BaseValue += 1;
            Assert.AreEqual(1, stat.Total);
            
            //Add 1 to modifed base value
            stat.ModifiedBase += 1;
            Assert.AreEqual(2, stat.Total);
            
            //Add 1 to bonus value
            stat.Bonus += 1;
            Assert.AreEqual(3, stat.Total);
            
            //Reset everything to 0
            stat.BaseValue = 0;
            stat.ModifiedBase = 0;
            stat.Bonus = 0;
            Assert.AreEqual(0, stat.Total);
        }
    }
}
