namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class UpgradeSpellReq : ICoreRequest
    {
        public byte Slot { get; }
        public bool IsEvolve { get; }

        public UpgradeSpellReq(byte slot, bool isEvolve)
        {
            Slot = slot;
            IsEvolve = isEvolve;
        }
    }
}
