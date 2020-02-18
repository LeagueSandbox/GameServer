namespace GameServerCore.Domain
{
    public interface IStackable
    {
        int MaxStacks { get; }
        int StackCount { get; }
        bool IncrementStackCount();
        bool DecrementStackCount();
        void SetStacks(int newStacks);
    }
}