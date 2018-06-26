using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SetHealthTest : BasePacket
    {
        public SetHealthTest(uint netId, short unk, float maxhp, float hp) 
            : base(PacketCmd.PKT_S2_C_SET_HEALTH, netId)
        {
            _buffer.Write((short)unk); // unk,maybe flags for physical/magical/true dmg
            _buffer.Write((float)maxhp);
            _buffer.Write((float)hp);
        }
    }
}