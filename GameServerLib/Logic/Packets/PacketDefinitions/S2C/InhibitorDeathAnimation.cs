using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class InhibitorDeathAnimation : BasePacket
    {
        public InhibitorDeathAnimation(Inhibitor inhi, GameObject killer)
            : base(PacketCmd.PKT_S2C_InhibitorDeathAnimation, inhi.NetId)
        {
            if (killer != null)
                buffer.Write((uint)killer.NetId);
            else
                buffer.Write((int)0);
            buffer.Write((int)0); //unk
        }
    }
}