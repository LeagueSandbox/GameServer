using System;
using LeagueSandbox.GameServer.Logic.GameObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameServerTests
{
    [TestClass]
    public class TestStats
    {
        [TestMethod]
        public void TestTotalAttackSpeed()
        {
            var stats = new Stats();

            Assert.AreEqual(0, stats.GetTotalAttackSpeed());

            stats.AttackSpeedFlat = 0.5f;

            Assert.AreEqual(0.5f, stats.GetTotalAttackSpeed());

            stats.AttackSpeedMultiplier.FlatBonus = 1.0f;

            Assert.AreEqual(1.0f, stats.GetTotalAttackSpeed());


            stats.AttackSpeedMultiplier.BaseValue = 0.0f;
            stats.AttackSpeedMultiplier.FlatBonus = 0.0f;

            Assert.AreEqual(0, stats.GetTotalAttackSpeed());
        }

        [TestMethod]
        public void TestLevelUp()
        {
            var stats = new Stats();

            Assert.AreEqual(0, stats.GetLevel());

            stats.LevelUp();

            Assert.AreEqual(1, stats.Level);

            Assert.AreEqual(0, stats.AttackDamage.BaseValue);
            Assert.AreEqual(0, stats.Armor.BaseValue);
            Assert.AreEqual(0, stats.HealthPoints.BaseValue);
            Assert.AreEqual(0, stats.HealthRegeneration.BaseValue);
            Assert.AreEqual(0, stats.MagicResist.BaseValue);
            Assert.AreEqual(0, stats.ManaPoints.BaseValue);
            Assert.AreEqual(0, stats.ManaRegeneration.BaseValue);

            stats.AdPerLevel = 1;
            stats.ArmorPerLevel = 2;
            stats.HealthPerLevel = 3;
            stats.HealthRegenerationPerLevel = 4;
            stats.MagicResistPerLevel = 5;
            stats.ManaPerLevel = 6;
            stats.ManaRegenerationPerLevel = 7;

            stats.LevelUp();

            Assert.AreEqual(2, stats.Level);

            Assert.AreEqual(1, stats.AttackDamage.BaseValue);
            Assert.AreEqual(2, stats.Armor.BaseValue);
            Assert.AreEqual(3, stats.HealthPoints.BaseValue);
            Assert.AreEqual(4, stats.HealthRegeneration.BaseValue);
            Assert.AreEqual(5, stats.MagicResist.BaseValue);
            Assert.AreEqual(6, stats.ManaPoints.BaseValue);
            Assert.AreEqual(7, stats.ManaRegeneration.BaseValue);
        }

        [TestMethod]
        public void TestGeneratingGold()
        {
            var stats = new Stats();

            Assert.IsFalse(stats.IsGeneratingGold());

            stats.SetGeneratingGold(true);

            Assert.IsTrue(stats.IsGeneratingGold());
        }
    }
}
