using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LeagueSandbox.GameServer.Content.Navigation
{
    public class NavigationHintGrid
    {
        public const uint HINT_NODE_COUNT = 900;

        public NavigationHintNode[] HintNodes { get; private set; } = new NavigationHintNode[HINT_NODE_COUNT];

        public NavigationHintGrid(BinaryReader br)
        {
            for (int i = 0; i < HINT_NODE_COUNT; i++)
            {
                this.HintNodes[i] = new NavigationHintNode(br);
            }
        }
    }
}
