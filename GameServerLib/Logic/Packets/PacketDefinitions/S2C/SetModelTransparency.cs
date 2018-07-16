using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SetModelTransparency : BasePacket
    {
        public SetModelTransparency(Game game, AttackableUnit u, float transparency, float transitionTime)
            : base(game, PacketCmd.PKT_S2C_SET_MODEL_TRANSPARENCY, u.NetId)
        {
            // Applied to Teemo's mushrooms for example
            Write((byte)0xDB); // Unk
            Write((byte)0x00); // Unk
            Write(transitionTime);
            Write(transparency); // 0.0 : fully transparent, 1.0 : fully visible
        }
    }
}