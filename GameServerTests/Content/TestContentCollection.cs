using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LeagueSandbox.GameServer.Logic.Items;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Content;

namespace GameServerTests
{
    [TestClass]
    public class TestContentCollection
    {
        [TestMethod]
        [DeploymentItem("Content", "Content")]
        public void TestItemContentCollection()
        {
            var collection = ItemContentCollection.LoadItemsFrom("Content/Data/LeagueSandbox-Default/Items");

            // This is the only way of accessing the items, but we really just need the first one
            ContentCollectionEntry entry = null;
            foreach (var item in collection)
            {
                entry = item.Value;
                break;
            }
            Assert.IsNotNull(entry);
            Assert.AreEqual("NonexistentValue", entry.SafeGetString("Nonexistent", "Section", "NonexistentValue"));
            Assert.AreEqual(true, entry.SafeGetBool("Dunno", "Whatever", true));
            Assert.AreEqual(false, entry.SafeGetBool("Dunno", "Whatever"));
            Assert.IsTrue(entry.ContentFormatVersion > 0);
            Assert.IsFalse(string.IsNullOrEmpty(entry.ResourcePath));
        }
    }
}
