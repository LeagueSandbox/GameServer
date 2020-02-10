using System.IO;

namespace LeagueSandbox.GameServer.Content.Navigation
{
    public class NavigationHintNode
    {
        public const uint DISTANCE_COUNT = 900;

        public float[] Distances { get; private set; } = new float[DISTANCE_COUNT];
        public NavigationGridLocator Locator { get; private set; }

        public NavigationHintNode(BinaryReader br)
        {
            for (int i = 0; i < DISTANCE_COUNT; i++)
            {
                this.Distances[i] = br.ReadSingle();
            }

            this.Locator = new NavigationGridLocator(br);
        }
    }
}
