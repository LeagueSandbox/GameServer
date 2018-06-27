using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SetHealthTest : BasePacket
    {
        public SetHealthTest(uint netId, short unk, float maxhp, float hp)
            : base(PacketCmd.PKT_S2_C_SET_HEALTH, netId)
        {
            _buffer.Write(unk); // unk,maybe flags for physical/magical/true dmg
            _buffer.Write(maxhp);
            _buffer.Write(hp);
        }
    }
}