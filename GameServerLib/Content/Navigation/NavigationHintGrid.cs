using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LeagueSandbox.GameServer.Content.Navigation
{
    public class NavigationHintGrid
    {
        public NavigationHintNode[] HintNodes { get; private set; } = new NavigationHintNode[900];

        public NavigationHintGrid(BinaryReader br)
        {
            for (int i = 0; i < this.HintNodes.Length; i++)
            {
                this.HintNodes[i] = new NavigationHintNode(br);
            }
        }
    }
}
