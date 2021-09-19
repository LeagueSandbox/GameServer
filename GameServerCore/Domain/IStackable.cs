namespace GameServerCore.Domain
{
    public interface IStackable
    {
        int MaxStacks { get; }
        int StackCount { get; }
        bool IncrementStackCount();
        bool DecrementStackCount(int stacksToRemove = 0);
        void SetStacks(int newStacks);
    }
}