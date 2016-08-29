using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.Chatbox;
using LeagueSandbox.GameServer.Logic;

namespace GameServerTests
{
    public class TestHelpers
    {
        public class DummyGame : Game
        {
            public DummyGame() : base() { }

            public void LoadItems()
            {
                ItemManager = ItemManager.LoadItems(this);
            }

            public void LoadChatboxManager()
            {
                ChatboxManager = new ChatboxManager(this);
            }

            public void LoadConfig()
            {
                Config = new Config("Settings/GameInfo.json");
            }
        }
    }
}
