using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class NpcDie : BasePacket
    {
        public NpcDie(Game game, AttackableUnit die, AttackableUnit killer)
            : base(game, PacketCmd.PKT_S2C_NPC_DIE, die.NetId)
        {
            Write(0);
            Write((byte)0);
            WriteNetId(killer);
            Write((byte)0); // unk
            Write((byte)7); // unk
            Write(0); // Flags?
        }
    }
}