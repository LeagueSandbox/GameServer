using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SetModelTransparency : BasePacket
    {
        public SetModelTransparency(AttackableUnit u, float transparency, float transitionTime)
            : base(PacketCmd.PKT_S2_C_SET_MODEL_TRANSPARENCY, u.NetId)
        {
            // Applied to Teemo's mushrooms for example
            _buffer.Write((byte)0xDB); // Unk
            _buffer.Write((byte)0x00); // Unk
            _buffer.Write((float)transitionTime);
            _buffer.Write((float)transparency); // 0.0 : fully transparent, 1.0 : fully visible
        }
    }
}