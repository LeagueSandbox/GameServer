using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class DestroyObject : BasePacket
    {
        public DestroyObject(AttackableUnit destroyer, AttackableUnit destroyed)
            : base(PacketCmd.PKT_S2_C_DESTROY_OBJECT, destroyer.NetId)
        {
            _buffer.Write((uint)destroyed.NetId);
        }
    }
}