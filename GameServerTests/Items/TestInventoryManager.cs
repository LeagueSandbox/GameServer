using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LeagueSandbox.GameServer.Logic.Items;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Content;

namespace GameServerTests
{
    [TestClass]
    public class InventoryManagerTests
    {
        [TestMethod]
        [DeploymentItem("Content", "Content")]
        public void TestAddAndGetItem()
        {
            var game = new DummyGame();
            game.LoadItems();

            var manager = InventoryManager.CreateInventory(game, null);

            // Add an item and make sure it gets added to the first (0) slot
            var item = manager.AddItem(game.ItemManager.GetItemType(2001));
            Assert.AreEqual(0, manager.GetItemSlot(item));

            // Make sure the added item and the one we fetch by it's slot are the same object
            var receivedItem = manager.GetItem(manager.GetItemSlot(item));
            Assert.AreEqual(item, receivedItem);

            // Add a trinket and check that it goes to the slot 7 (so index 6)
            item = manager.AddItem(game.ItemManager.GetItemType(3361));
            receivedItem = manager.GetItem(6);
            Assert.AreEqual(item, receivedItem);

            // Check that we get null back when we try to add another trinket
            item = manager.AddItem(game.ItemManager.GetItemType(3352));
            Assert.IsNull(item);

            // Add 5 more items and check that each of them get added
            for (var i = 0; i < 5; i++)
            {
                item = manager.AddItem(game.ItemManager.GetItemType(4001 + i));
                receivedItem = manager.GetItem(manager.GetItemSlot(item));
                Assert.AreEqual(item, receivedItem);
            }

            // Check that we get null back when we try to add a new item
            item = manager.AddItem(game.ItemManager.GetItemType(4007));
            Assert.IsNull(item);
        }

        [TestMethod]
        [DeploymentItem("Content", "Content")]
        public void TestRemoveItem()
        {
            var game = new DummyGame();
            game.LoadItems();

            var manager = InventoryManager.CreateInventory(game, null);

            // Add an item and make sure it gets added to the first (0) slot
            var item = manager.AddItem(game.ItemManager.GetItemType(2001));
            Assert.AreEqual(0, manager.GetItemSlot(item));

            // Remove the item and make sure it doesn't exist anymore in the inventory
            manager.RemoveItem(manager.GetItemSlot(item));
            Assert.IsNull(manager.GetItem(0));
        }

        [TestMethod]
        [DeploymentItem("Content", "Content")]
        public void TestSwapItems()
        {
            var game = new DummyGame();
            game.LoadItems();

            var manager = InventoryManager.CreateInventory(game, null);

            // Add 3 items and make sure they get proper slots
            var item1 = manager.AddItem(game.ItemManager.GetItemType(4001));
            var item2 = manager.AddItem(game.ItemManager.GetItemType(4002));
            var item3 = manager.AddItem(game.ItemManager.GetItemType(4003));
            Assert.AreEqual(0, manager.GetItemSlot(item1));
            Assert.AreEqual(1, manager.GetItemSlot(item2));
            Assert.AreEqual(2, manager.GetItemSlot(item3));

            // Swap 0 and 2 around and make sure their slots have swapped
            manager.SwapItems(0, 2);
            Assert.AreEqual(2, manager.GetItemSlot(item1));
            Assert.AreEqual(item1, manager.GetItem(2));
            Assert.AreEqual(0, manager.GetItemSlot(item3));
            Assert.AreEqual(item3, manager.GetItem(0));

            // Swap 0 and 1 around and make sure their slots have swapped
            manager.SwapItems(0, 1);
            Assert.AreEqual(manager.GetItemSlot(item3), 1);
            Assert.AreEqual(item3, manager.GetItem(1));
            Assert.AreEqual(manager.GetItemSlot(item2), 0);
            Assert.AreEqual(item2, manager.GetItem(0));

            // Swap with null and make sure it works
            manager.SwapItems(0, 3);
            Assert.IsNull(manager.GetItem(0));
            Assert.AreEqual(manager.GetItemSlot(item2), 3);
            Assert.AreEqual(item2, manager.GetItem(3));

            // Try to swap to the trinket slot and make sure it fails
            var failed = false;
            try
            {
                manager.SwapItems(3, 6);
                Assert.Fail("This should have failed");
            }
            catch
            {
                failed = true;
            }
            Assert.IsTrue(failed);
        }

        [TestMethod]
        [DeploymentItem("Content", "Content")]
        public void TestGetAvailableItems()
        {
            var game = new DummyGame();
            game.LoadItems();

            var manager = InventoryManager.CreateInventory(game, null);

            var zephyrId = 3172;
            var componentId1 = 3101;
            var componentId2 = 1037;

            // Get zephyr and make sure we have no items available to it's recipe
            var zephyr = game.ItemManager.GetItemType(zephyrId);
            Assert.AreEqual(0, manager.GetAvailableItems(zephyr.Recipe).Count);

            // Add a component and make sure we get it from the available items function
            var component1 = manager.AddItem(game.ItemManager.GetItemType(componentId1));
            var available = manager.GetAvailableItems(zephyr.Recipe);
            Assert.AreEqual(1, available.Count);
            Assert.AreEqual(component1, available[0]);

            // Add another component and make sure we get that as well
            var component2 = manager.AddItem(game.ItemManager.GetItemType(componentId2));
            available = manager.GetAvailableItems(zephyr.Recipe);
            Assert.AreEqual(2, available.Count);
            Assert.AreEqual(component1, available[0]);
            Assert.AreEqual(component2, available[1]);

            // Remove the first component and make sure we still have everything correctly
            manager.RemoveItem(manager.GetItemSlot(component1));
            available = manager.GetAvailableItems(zephyr.Recipe);
            Assert.AreEqual(1, available.Count);
            Assert.AreEqual(component2, available[0]);
        }
    }

    public class DummyGame : Game
    {
        public DummyGame() : base() { }

        public void LoadItems()
        {
            ItemManager = ItemManager.LoadItems(this);
        }
    }
}
