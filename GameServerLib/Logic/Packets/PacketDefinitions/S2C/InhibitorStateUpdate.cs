using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.Buildings.AnimatedBuildings;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class InhibitorStateUpdate : BasePacket
    {
        public InhibitorStateUpdate(Game game, Inhibitor inhi)
            : base(game, PacketCmd.PKT_S2C_INHIBITOR_STATE, inhi.NetId)
        {
            Write((byte)inhi.InhibitorState);
            Write((byte)0);
            Write((byte)0);
        }
    }
}