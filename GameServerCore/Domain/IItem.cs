namespace GameServerCore.Domain
{
    public interface IItem
    {
        byte StackSize { get; }
        int TotalPrice { get; }
        IItemType ItemType { get; }
    }
}
