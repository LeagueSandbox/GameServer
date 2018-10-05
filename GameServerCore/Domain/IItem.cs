namespace GameServerCore.Domain
{
    public interface IItem: IStackable
    {
        int TotalPrice { get; }
        IItemType ItemType { get; }
    }
}
