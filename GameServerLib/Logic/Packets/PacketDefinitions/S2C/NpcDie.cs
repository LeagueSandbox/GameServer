using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class NpcDie : BasePacket
    {
        public NpcDie(Unit die, Unit killer) 
            : base(PacketCmd.PKT_S2C_NPC_Die, die.NetId)
        {
            buffer.Write((int)0);
            buffer.Write((byte)0);
            buffer.Write(killer.NetId);
            buffer.Write((byte)0); // unk
            buffer.Write((byte)7); // unk
            buffer.Write((int)0); // Flags?
        }
    }
}