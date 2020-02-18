using System.IO;

namespace LeagueSandbox.GameServer.Content.Navigation
{
    public class NavigationRegionTagTableGroupTag
    {
        public uint Name { get; private set; }
        public (uint name1, uint name2)[] Values { get; private set; } = new (uint name1, uint name2)[16];

        public NavigationRegionTagTableGroupTag(BinaryReader br)
        {
            this.Name = br.ReadUInt32();

            for (int i = 0; i < this.Values.Length; i++)
            {
                this.Values[i] = (br.ReadUInt32(), br.ReadUInt32());
            }
        }
    }
}