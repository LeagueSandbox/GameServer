using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class DestroyObject : BasePacket
    {
        public DestroyObject(Game game, AttackableUnit destroyer, AttackableUnit destroyed)
            : base(game, PacketCmd.PKT_S2C_DESTROY_OBJECT, destroyer.NetId)
        {
            WriteNetId(destroyed);
        }
    }
}