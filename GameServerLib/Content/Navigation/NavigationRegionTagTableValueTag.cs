using System.IO;

namespace LeagueSandbox.GameServer.Content.Navigation
{
    public struct NavigationRegionTagTableValueTag
    {
        public uint Name1 { get; private set; }
        public uint Name2 { get; private set; }

        public NavigationRegionTagTableValueTag(BinaryReader br)
        {
            this.Name1 = br.ReadUInt32();
            this.Name2 = br.ReadUInt32();
        }
    }
}
