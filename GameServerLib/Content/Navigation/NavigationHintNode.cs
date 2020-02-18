using System.IO;

namespace LeagueSandbox.GameServer.Content.Navigation
{
    public class NavigationHintNode
    {
        public float[] Distances { get; private set; } = new float[900];
        public NavigationGridLocator Locator { get; private set; }

        public NavigationHintNode(BinaryReader br)
        {
            for (int i = 0; i < this.Distances.Length; i++)
            {
                this.Distances[i] = br.ReadSingle();
            }

            this.Locator = new NavigationGridLocator(br);
        }
    }
}
