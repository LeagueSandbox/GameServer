using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.Chatbox;
using LeagueSandbox.GameServer.Logic;
using LeagueSandbox.GameServer.Logic.Packets;

namespace GameServerTests
{
    public class TestHelpers
    {
        public class DummyGame : Game
        {
            public DummyGame() : base() { }

            public void Initialize()
            {
                Config = new Config("Settings/GameInfo.json");

                ItemManager = ItemManager.LoadItems(this);
                ChatboxManager = new ChatboxManager(this);
                PacketHandlerManager = new PacketHandlerManager(this);
                PacketNotifier = new PacketNotifier(this);
            }
        }
    }
}
