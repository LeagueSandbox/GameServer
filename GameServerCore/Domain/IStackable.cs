namespace GameServerCore.Domain
{
    public interface IStackable
    {
        byte StackCount { get; }
        bool IncrementStackCount();
        bool DecrementStackCount();
        void SetStacks(byte newStacks);
    }
}