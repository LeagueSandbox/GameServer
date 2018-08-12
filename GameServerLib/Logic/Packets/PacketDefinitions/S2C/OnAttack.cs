using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class OnAttack : BasePacket
    {
        public OnAttack(AttackableUnit attacker, AttackableUnit attacked, AttackType attackType)
            : base(PacketCmd.PKT_S2C_OnAttack, attacker.NetId)
        {
            buffer.Write((byte)attackType);
            buffer.Write(attacked.X);
            buffer.Write(attacked.GetZ());
            buffer.Write(attacked.Y);
            buffer.Write(attacked.NetId);
        }
    }
}