using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.Buildings.AnimatedBuildings;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class InhibitorDeathAnimation : BasePacket
    {
        public InhibitorDeathAnimation(Inhibitor inhi, GameObject killer)
            : base(PacketCmd.PKT_S2C_INHIBITOR_DEATH_ANIMATION, inhi.NetId)
        {
            WriteNetId(killer);
            Write(0); //unk
        }
    }
}