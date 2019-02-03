using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class ItemsSwappedResponse : ICoreResponse
    {
        public IChampion Champ { get; }
        public byte FromSlot { get; }
        public byte ToSlot { get; }
        public ItemsSwappedResponse(IChampion c, byte fromSlot, byte toSlot)
        {
            Champ = c;
            FromSlot = fromSlot;
            ToSlot = toSlot;
        }
    }
}