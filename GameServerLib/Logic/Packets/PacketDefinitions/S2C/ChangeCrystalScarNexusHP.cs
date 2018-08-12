using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class ChangeCrystalScarNexusHP : BasePacket
    {
        public ChangeCrystalScarNexusHP(TeamId team, int hp)
            : base(PacketCmd.PKT_S2C_ChangeCrystalScarNexusHP)
        {
            buffer.Write((uint)team);
            buffer.Write(hp);
        }
    }
}