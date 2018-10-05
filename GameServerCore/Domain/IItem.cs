namespace GameServerCore.Domain
{
    public interface IItem: IStackable
    {
        int TotalPrice { get; }
        IItemData ItemData { get; }
    }
}
