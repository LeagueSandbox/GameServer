using System;
using GameServerCore.Packets.Interfaces;
using LeagueSandbox.GameServer;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.Inventory;
using LeagueSandbox.GameServer.Scripting.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LeagueSandbox.GameServerTests.Tests.Items
{
    [TestClass]
    public class InventoryManagerTests
    {
        readonly String path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "/../../../../Content/";
        [TestMethod]
        public void TestAddAndGetItem()
        {
            var itemManager = new ItemManager();
            itemManager.AddItems(ItemContentCollection.LoadItemsFrom(path + "LeagueSandbox-Default/Items"));

            IPacketNotifier _packetNotifier = new Game().PacketNotifier;
            var inventoryManager = InventoryManager.CreateInventory(_packetNotifier);

            // Add an item and make sure it gets added to the first (0) slot
            var item = inventoryManager.AddItem(itemManager.GetItemType(2001));
            Assert.AreEqual(0, inventoryManager.GetItemSlot(item.Key));

            // Make sure the added item and the one we fetch by it's slot are the same object
            var receivedItem = inventoryManager.GetItem(inventoryManager.GetItemSlot(item.Key));
            Assert.AreEqual(item.Key, receivedItem);

            // Add a trinket and check that it goes to the slot 7 (so index 6)
            item = inventoryManager.AddItem(itemManager.GetItemType(3361));
            receivedItem = inventoryManager.GetItem(6);
            Assert.AreEqual(item.Key, receivedItem);

            // Check that we get null back when we try to add another trinket
            item = inventoryManager.AddItem(itemManager.GetItemType(3352));
            Assert.IsNull(item.Key);

            // Add 5 more items and check that each of them get added
            for (var i = 0; i < 5; i++)
            {
                item = inventoryManager.AddItem(itemManager.GetItemType(4001 + i));
                receivedItem = inventoryManager.GetItem(inventoryManager.GetItemSlot(item.Key));
                Assert.AreEqual(item.Key, receivedItem);
            }

            // Check that we get null back when we try to add a new item
            item = inventoryManager.AddItem(itemManager.GetItemType(4007));
            Assert.IsNull(item.Key);
        }

        [TestMethod]
        public void TestItemStacking()
        {
            var itemManager = new ItemManager();
            itemManager.AddItems(ItemContentCollection.LoadItemsFrom(path + "LeagueSandbox-Default/Items"));

            IPacketNotifier _packetNotifier = new Game().PacketNotifier;
            var manager = InventoryManager.CreateInventory(_packetNotifier);

            // Get two stacking item types
            var itemType1 = itemManager.GetItemType(2038);
            var itemType2 = itemManager.GetItemType(2040);

            // Add items
            var item1 = manager.AddItem(itemType1);
            var item2 = manager.AddItem(itemType2);

            // Check existance of items
            Assert.AreEqual(item1.Key, manager.GetItem(0));
            Assert.AreEqual(item2.Key, manager.GetItem(1));

            // Check stack sizes
            Assert.AreEqual(1, item1.Key.StackCount);
            Assert.AreEqual(1, item2.Key.StackCount);

            // Stack the second item, and make sure the second gets stacked
            for (var i = 0; i < itemType2.MaxStacks - 1; i++)
            {
                var item2Reference = manager.AddItem(itemType2);
                Assert.AreEqual(item2.Key, item2Reference.Key);
                Assert.AreEqual(1 + i + 1, item2.Key.StackCount);
            }

            // Make sure the first item's stack is unchanged
            Assert.AreEqual(1, item1.Key.StackCount);

            // Make sure we can't add any more of the second item to the stack
            var shouldBeNull = manager.AddItem(itemType2);
            Assert.IsNull(shouldBeNull.Key);
        }

        [TestMethod]
        public void TestSetExtraItem()
        {
            var itemManager = new ItemManager();
            itemManager.AddItems(ItemContentCollection.LoadItemsFrom(path + "LeagueSandbox-Default/Items"));

            IPacketNotifier _packetNotifier = new Game().PacketNotifier;
            var manager = InventoryManager.CreateInventory(_packetNotifier);

            // Add an item and make sure it exists in the proper slot
            var item = manager.SetExtraItem(7, itemManager.GetItemType(2001));
            var slot = manager.GetItemSlot(item);
            Assert.AreEqual(7, slot);

            // Try to add an extra item to an invalid slot, make sure it fails
            try
            {
                manager.SetExtraItem(6, itemManager.GetItemType(2001));
                Assert.Fail("This should fail");
            }
            catch (Exception e)
            {
                Assert.AreEqual("Invalid extra item slot—must be greater than base inventory size!", e.Message);
            }
        }

        [TestMethod]
        public void TestGetItemSlot()
        {
            var itemManager = new ItemManager();
            itemManager.AddItems(ItemContentCollection.LoadItemsFrom(path + "LeagueSandbox-Default/Items"));

            IPacketNotifier _packetNotifier = new Game().PacketNotifier;
            var manager = InventoryManager.CreateInventory(_packetNotifier);

            // Add an item, and make sure the slot is right
            var item = manager.AddItem(itemManager.GetItemType(2001));
            var slot = manager.GetItemSlot(item.Key);
            Assert.AreEqual(0, slot);

            // Remove the item, and make sure the item slot fetching fails
            manager.RemoveItem(slot, null);
            try
            {
                var fail = manager.GetItemSlot(item.Key);
                Assert.Fail("This should fail");
            }
            catch (Exception e)
            {
                Assert.AreEqual("Specified item doesn't exist in the inventory!", e.Message);
            }

            // Add an extra item to a specific slot, and make sure this still works
            var extraItem = manager.SetExtraItem(7, itemManager.GetItemType(4002));
            var extraSlot = manager.GetItemSlot(extraItem);
            Assert.AreEqual(7, extraSlot);
        }

        [TestMethod]
        public void TestRemoveItem()
        {
            var itemManager = new ItemManager();
            itemManager.AddItems(ItemContentCollection.LoadItemsFrom(path + "LeagueSandbox-Default/Items"));

            IPacketNotifier _packetNotifier = new Game().PacketNotifier;
            var manager = InventoryManager.CreateInventory(_packetNotifier);

            // Add an item and make sure it gets added to the first (0) slot
            var item = manager.AddItem(itemManager.GetItemType(2001));
            Assert.AreEqual(0, manager.GetItemSlot(item.Key));

            // Remove the item and make sure it doesn't exist anymore in the inventory
            manager.RemoveItem(manager.GetItemSlot(item.Key));
            Assert.IsNull(manager.GetItem(0));

            // Add a new item and make sure it's added to the first (0) slot
            item = manager.AddItem(itemManager.GetItemType(2001));
            Assert.AreEqual(0, manager.GetItemSlot(item.Key));

            // Remove the item another way and make sure it doesn't exist anymore in the inventory
            manager.RemoveItem(item.Key);
            Assert.IsNull(manager.GetItem(0));
        }

        [TestMethod]
        public void TestSwapItems()
        {
            var itemManager = new ItemManager();
            itemManager.AddItems(ItemContentCollection.LoadItemsFrom(path + "LeagueSandbox-Default/Items"));

            IPacketNotifier _packetNotifier = new Game().PacketNotifier;
            var manager = InventoryManager.CreateInventory(_packetNotifier);

            // Add 3 items and make sure they get proper slots
            var item1 = manager.AddItem(itemManager.GetItemType(4001));
            var item2 = manager.AddItem(itemManager.GetItemType(4002));
            var item3 = manager.AddItem(itemManager.GetItemType(4003));
            Assert.AreEqual(0, manager.GetItemSlot(item1.Key));
            Assert.AreEqual(1, manager.GetItemSlot(item2.Key));
            Assert.AreEqual(2, manager.GetItemSlot(item3.Key));

            // Swap 0 and 2 around and make sure their slots have swapped
            manager.SwapItems(0, 2);
            Assert.AreEqual(2, manager.GetItemSlot(item1.Key));
            Assert.AreEqual(item1.Key, manager.GetItem(2));
            Assert.AreEqual(0, manager.GetItemSlot(item3.Key));
            Assert.AreEqual(item3.Key, manager.GetItem(0));

            // Swap 0 and 1 around and make sure their slots have swapped
            manager.SwapItems(0, 1);
            Assert.AreEqual(manager.GetItemSlot(item3.Key), 1);
            Assert.AreEqual(item3.Key, manager.GetItem(1));
            Assert.AreEqual(manager.GetItemSlot(item2.Key), 0);
            Assert.AreEqual(item2.Key, manager.GetItem(0));

            // Swap with null and make sure it works
            manager.SwapItems(0, 3);
            Assert.IsNull(manager.GetItem(0));
            Assert.AreEqual(manager.GetItemSlot(item2.Key), 3);
            Assert.AreEqual(item2.Key, manager.GetItem(3));

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
        public void TestGetAvailableItems()
        {
            var itemManager = new ItemManager();
            itemManager.ResetItems();
            itemManager.AddItems(ItemContentCollection.LoadItemsFrom(path + "LeagueSandbox-Default/Items"));

            IPacketNotifier _packetNotifier = new Game().PacketNotifier;
            var manager = InventoryManager.CreateInventory(_packetNotifier);

            var zephyrId = 3172;
            var componentId1 = 3101;
            var componentId2 = 1037;

            // Get zephyr and make sure we have no items available to it's recipe
            var zephyr = itemManager.GetItemType(zephyrId);
            var availableItems = manager.GetAvailableItems(zephyr.Recipe.GetItems());
            Assert.AreEqual(0, availableItems.Count);

            // Add a component and make sure we get it from the available items function
            var component1 = manager.AddItem(itemManager.GetItemType(componentId1));
            var available = manager.GetAvailableItems(zephyr.Recipe.GetItems());
            Assert.AreEqual(1, available.Count);
            Assert.AreEqual(component1.Key, available[0]);

            // Add another component and make sure we get that as well
            var component2 = manager.AddItem(itemManager.GetItemType(componentId2));
            available = manager.GetAvailableItems(zephyr.Recipe.GetItems());
            Assert.AreEqual(2, available.Count);
            Assert.AreEqual(component1.Key, available[0]);
            Assert.AreEqual(component2.Key, available[1]);

            // Remove the first component and make sure we still have everything correctly
            manager.RemoveItem(manager.GetItemSlot(component1.Key));
            available = manager.GetAvailableItems(zephyr.Recipe.GetItems());
            Assert.AreEqual(1, available.Count);
            Assert.AreEqual(component2.Key, available[0]);

            // Remove the other comopnent as well
            manager.RemoveItem(manager.GetItemSlot(component2.Key));

            // Add an unrelated item and make sure it exists
            var unrelated = manager.AddItem(itemManager.GetItemType(4001));
            Assert.IsNotNull(manager.GetItem(manager.GetItemSlot(unrelated.Key)));

            // Make sure we have no available items, even though there are some in the inventory
            available = manager.GetAvailableItems(zephyr.Recipe.GetItems());
            Assert.AreEqual(0, available.Count);
        }
    }
}
