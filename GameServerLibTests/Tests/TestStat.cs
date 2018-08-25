using LeagueSandbox.GameServer.GameObjects.Stats;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LeagueSandbox.GameServerTests.Tests
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
            stat.BaseBonus += 1;
            Assert.AreEqual(2, stat.Total);

            stat.PercentBaseBonus = 1.0f;
            Assert.AreEqual(4, stat.Total);

            //Add 1 to bonus value
            stat.FlatBonus += 1;
            Assert.AreEqual(5, stat.Total);

            //Add set to 100% bonus value
            stat.PercentBonus = 1.0f;
            Assert.AreEqual(10, stat.Total);

            //Reset everything to 0
            stat.BaseValue = 0;
            stat.BaseBonus = 0;
            stat.PercentBaseBonus = 0;
            stat.FlatBonus = 0;
            stat.PercentBonus = 0;
            Assert.AreEqual(0, stat.Total);
        }

        [TestMethod]
        public void TestModified()
        {
            //Create Stat with nothing modified (so everything = 0)
            var stat = new Stat();

            //Test if it's not modified
            Assert.IsFalse(stat.Modified);

            //Modify stat
            stat.FlatBonus = 10;

            //Test if it's modified
            Assert.IsTrue(stat.Modified);

            //Set back to 0
            stat.FlatBonus = 0;

            //Test if it still modified
            Assert.IsTrue(stat.Modified);
        }
    }
}
