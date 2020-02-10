using System.IO;

namespace LeagueSandbox.GameServer.Content.Navigation
{
    public class NavigationRegionTagTableGroupTag
    {
        public const uint VALUE_COUNT = 16;

        public uint Name { get; private set; }
        public NavigationRegionTagTableValueTag[] Values { get; private set; } = new NavigationRegionTagTableValueTag[VALUE_COUNT];

        public NavigationRegionTagTableGroupTag(BinaryReader br)
        {
            this.Name = br.ReadUInt32();

            for (int i = 0; i < VALUE_COUNT; i++)
            {
                this.Values[i] = new NavigationRegionTagTableValueTag(br);
            }
        }
    }
}
