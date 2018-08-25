using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class SetModelTransparency : BasePacket
    {
        public SetModelTransparency(IAttackableUnit u, float transparency, float transitionTime)
            : base(PacketCmd.PKT_S2C_SET_MODEL_TRANSPARENCY, u.NetId)
        {
            // Applied to Teemo's mushrooms for example
            Write((byte)0xDB); // Unk
            Write((byte)0x00); // Unk
            Write(transitionTime);
            Write(transparency); // 0.0 : fully transparent, 1.0 : fully visible
        }
    }
}