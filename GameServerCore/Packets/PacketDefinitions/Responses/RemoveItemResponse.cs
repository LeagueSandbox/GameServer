using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class RemoveItemResponse : ICoreResponse
    {
        public IChampion Champ { get; }
        public byte Slot { get; }
        public byte Remaining { get; }
        public RemoveItemResponse(IChampion c, byte slot, byte remaining)
        {
            Champ = c;
            Slot = slot;
            Remaining = remaining;
        }
    }
};