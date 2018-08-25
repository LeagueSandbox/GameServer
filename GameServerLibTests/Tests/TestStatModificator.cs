using LeagueSandbox.GameServer.GameObjects.Stats;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LeagueSandbox.GameServerTests.Tests
{
    [TestClass]
    public class TestStatModificator
    {
        [TestMethod]
        public void TestStatModificator1()
        {
            //Create new stat modificator with all value to 0
            var statModificator = new StatModifier();
            //Test if not modificated
            Assert.IsFalse(statModificator.StatModified);

            //Change values
            statModificator.BaseBonus = 1;
            statModificator.PercentBaseBonus = 2;
            statModificator.FlatBonus = 3;
            statModificator.PercentBonus = 4;

            //Test values
            Assert.AreEqual(1, statModificator.BaseBonus);
            Assert.AreEqual(2, statModificator.PercentBaseBonus);
            Assert.AreEqual(3, statModificator.FlatBonus);
            Assert.AreEqual(4, statModificator.PercentBonus);

            //Test if modificated
            Assert.IsTrue(statModificator.StatModified);
        }
    }
}
