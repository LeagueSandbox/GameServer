using Microsoft.VisualStudio.TestTools.UnitTesting;
using LeagueSandbox.GameServer.Logic.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Core.Logic;
using BlowFishCS;
using LeagueSandbox.GameServer.Logic.Maps;
using LeagueSandbox.GameServer.Logic.Packets;
using ENet;

namespace TestLeagueSandbox.GameServer.Logic.GameObjects
{
    [TestClass]
    public class TestAnnounce
    {
        [TestMethod]
        public void TestAnnounces()
        {
            var game = new DummyGame();
            var announce = new Announce(game, 1000, Announces.WelcomeToSR, true);
            Assert.AreEqual(1000, announce.GetEventTime());
            Assert.AreEqual(false, announce.IsAnnounced());
            announce.Execute();
            Assert.AreEqual(true, announce.IsAnnounced());

            announce = new Announce(game, 1000, Announces.WelcomeToSR, false);
            announce.Execute();
        }
    }
    class DummyGame : Game
    {
        public DummyGame()
        {
            _blowfish = new BlowFish(Encoding.Default.GetBytes("UnitTests stinks"));
            _packetHandlerManager = new PacketHandlerManager(this);
            _map = new Map(this, 1, 1, 1, true, 1);
            _packetNotifier = new PacketNotifier(this);
            _server = new Host();
            _server.Create(12345, 123);
        }
    }
}
