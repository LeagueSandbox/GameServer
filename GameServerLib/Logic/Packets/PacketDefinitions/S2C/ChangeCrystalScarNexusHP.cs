using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class ChangeCrystalScarNexusHp : BasePacket
    {
        public ChangeCrystalScarNexusHp(TeamId team, int hp)
            : base(PacketCmd.PKT_S2_C_CHANGE_CRYSTAL_SCAR_NEXUS_HP)
        {
            _buffer.Write((uint)team);
            _buffer.Write(hp);
        }
    }
}