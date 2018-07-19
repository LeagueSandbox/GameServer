using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class OnAttack : BasePacket
    {
        public OnAttack(Game game, AttackableUnit attacker, AttackableUnit attacked, AttackType attackType)
            : base(game, PacketCmd.PKT_S2C_ON_ATTACK, attacker.NetId)
        {
            Write((byte)attackType);
            Write(attacked.X);
            Write(attacked.GetZ());
            Write(attacked.Y);
            WriteNetId(attacked);
        }
    }
}