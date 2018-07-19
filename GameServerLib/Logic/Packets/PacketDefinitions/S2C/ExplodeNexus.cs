using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.Buildings.AnimatedBuildings;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class ExplodeNexus : BasePacket
    {
        public ExplodeNexus(Game game, Nexus nexus)
            : base(game, PacketCmd.PKT_S2C_EXPLODE_NEXUS, nexus.NetId)
        {
            // animation ID?
            Write((byte)0xE7);
            Write((byte)0xF9);
            Write((byte)0x00);
            Write((byte)0x40);
            // unk
            Write((byte)0x00);
            Write((byte)0x00);
            Write((byte)0x00);
            Write((byte)0x00);
        }
    }
}