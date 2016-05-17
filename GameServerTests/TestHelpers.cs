﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Content;

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
                ChatboxManager = new LeagueSandbox.GameServer.Logic.Chatbox.ChatboxManager(this);
            }

            public void LoadBuffManager()
            {
                BuffManager = new LeagueSandbox.GameServer.Logic.GameObjects.BuffManager(this);
            }
        }
    }
}
