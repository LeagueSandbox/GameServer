using System.IO;

namespace LeagueSandbox.GameServer.Content.Navigation
{
    public class NavigationRegionTagTableGroupTag
    {
        public uint Name { get; private set; }
        public NavigationRegionTagTableValueTag[] Values { get; private set; } = new NavigationRegionTagTableValueTag[16];

        public NavigationRegionTagTableGroupTag(BinaryReader br)
        {
            this.Name = br.ReadUInt32();

            for (int i = 0; i < this.Values.Length; i++)
            {
                this.Values[i] = new NavigationRegionTagTableValueTag(br);
            }
        }
    }
}