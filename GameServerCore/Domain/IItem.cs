namespace GameServerCore.Domain
{
    public interface IItem
    {
        byte StackSize { get; }
        // todo: Unused atm, is this needed?
        int TotalPrice { get; }
        IItemType ItemType { get; }
    }
}
