using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class SetCooldownResponse : ICoreResponse
    {
        public IChampion Champ { get; }
        public byte SlotId { get; }
        public float CurrentCd { get; }
        public float TotalCd { get; }
        public SetCooldownResponse(IChampion c, byte slotId, float currentCd, float totalCd)
        {
            Champ = c;
            SlotId = slotId;
            CurrentCd = currentCd;
            TotalCd = totalCd;
        }
    }
}