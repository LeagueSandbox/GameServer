using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LeagueSandbox.GameServer.Content.Navigation
{
    public class NavigationRegionTagTable
    {
        public NavigationRegionTagTableGroupTag[] Groups { get; private set; }

        public NavigationRegionTagTable(BinaryReader br, uint groupCount)
        {
            this.Groups = new NavigationRegionTagTableGroupTag[groupCount];

            for(int i = 0; i < this.Groups.Length; i++)
            {
                this.Groups[i] = new NavigationRegionTagTableGroupTag(br);
            }
        }
    }
}
