namespace GameServerCore.Domain.GameObjects
{
    public interface IPet : IMinion
    {
        public IBuff CloneBuff { get; }
        public float LifeTime { get; }
        public bool CloneInventory { get; }
        public bool DoFade { get; }
        public bool ShowMinimapIfClone { get; }
    }
}
