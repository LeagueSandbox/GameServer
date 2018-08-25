using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LeagueSandbox.GameServerTests.Tests
{
    [TestClass]
    public class TestGameplayElements
    {
        // TODO implement things below when things work
        [TestMethod]
        public void TestTargetedDash()
        {
            // var champ = new Champion("Vi", 0, new RuneCollection()); Unit which will dash
            // var minion = new Minion(MinionSpawnType.MINION_TYPE_SUPER, MinionSpawnPosition.SPAWN_BLUE_MID); unit will be dashed to
            // champ.setPosition(100, 100);
            // minion.setPosition(200, 200);
            // champ.DashToTarget(minion, 150, 0, 0, 0);

            // wait until both are colliding, then:
            // Assert.IsFalse(champ.IsDashing);
        }

        [TestMethod]
        public void TestTargetedProjectile()
        {
            // var champ = new Champion("Ezreal", 0, new RuneCollection()); // Unit which will attack
            // var projectile = new Projectile(100, 100, 50, champ, champ, null, 150, 0); // the projectile
            // champ.setPosition(200, 200);

            // wait until both are colliding, then:
            // Assert.IsTrue(projectile.isToRemove());
        }
    }
}
