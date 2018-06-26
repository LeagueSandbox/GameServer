using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class InhibitorDeathAnimation : BasePacket
    {
        public InhibitorDeathAnimation(Inhibitor inhi, GameObject killer)
            : base(PacketCmd.PKT_S2_C_INHIBITOR_DEATH_ANIMATION, inhi.NetId)
        {
            if (killer != null)
                _buffer.Write((uint)killer.NetId);
            else
                _buffer.Write((int)0);
            _buffer.Write((int)0); //unk
        }
    }
}