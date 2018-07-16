using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class ChangeCrystalScarNexusHp : BasePacket
    {
        public ChangeCrystalScarNexusHp(Game game, TeamId team, int hp)
            : base(game, PacketCmd.PKT_S2C_CHANGE_CRYSTAL_SCAR_NEXUS_HP)
        {
            Write((uint)team);
            Write(hp);
        }
    }
}