namespace GameServerCore.Domain
{
    public interface IItem
    {
        byte StackSize { get; }
        int TotalPrice { get; }
        IItemData ItemData { get; }
    }
}
