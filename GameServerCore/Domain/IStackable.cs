namespace GameServerCore.Domain
{
    public interface IStackable
    {
        byte StackCount { get; }
        bool WithinBounds(byte stackCount);
        void SetStacks(byte newStacks);
    }
}