using LeagueSandbox.GameServer.Content;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GameServerTests
{
    [TestClass]
    public class TestContentCollection
    {
        readonly String path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/../../../../Content/";


        [TestMethod]
        [DeploymentItem("Content", "Content")]
        public void TestItemContentCollection()
        {
            var collection = ItemContentCollection.LoadItemsFrom(path + "LeagueSandbox-Default/Items");

            // This is the only way of accessing the items, but we really just need the first one
            ContentCollectionEntry entry = null;
            foreach (var item in collection)
            {
                entry = item.Value;
                break;
            }
            Assert.IsNotNull(entry);
            Assert.IsTrue(entry.ContentFormatVersion > 0);
            Assert.IsFalse(string.IsNullOrEmpty(entry.ResourcePath));
        }
    }
}
