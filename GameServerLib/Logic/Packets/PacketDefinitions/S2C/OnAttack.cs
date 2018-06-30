using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class OnAttack : BasePacket
    {
        public OnAttack(AttackableUnit attacker, AttackableUnit attacked, AttackType attackType)
            : base(PacketCmd.PKT_S2C_ON_ATTACK, attacker.NetId)
        {
            _buffer.Write((byte)attackType);
            _buffer.Write(attacked.X);
            _buffer.Write(attacked.GetZ());
            _buffer.Write(attacked.Y);
            _buffer.Write(attacked.NetId);
        }
    }
}