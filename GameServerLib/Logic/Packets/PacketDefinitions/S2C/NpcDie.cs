using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class NpcDie : BasePacket
    {
        public NpcDie(AttackableUnit die, AttackableUnit killer) 
            : base(PacketCmd.PKT_S2_C_NPC_DIE, die.NetId)
        {
            _buffer.Write((int)0);
            _buffer.Write((byte)0);
            _buffer.Write(killer.NetId);
            _buffer.Write((byte)0); // unk
            _buffer.Write((byte)7); // unk
            _buffer.Write((int)0); // Flags?
        }
    }
}