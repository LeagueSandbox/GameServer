using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class SetHealthTest : BasePacket
    {
        public SetHealthTest(uint netId, short unk, float maxhp, float hp)
            : base(PacketCmd.PKT_S2C_SET_HEALTH, netId)
        {
            Write(unk); // unk,maybe flags for physical/magical/true dmg
            Write(maxhp);
            Write(hp);
        }
    }
}