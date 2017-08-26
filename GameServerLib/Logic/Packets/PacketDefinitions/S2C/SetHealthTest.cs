using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SetHealthTest : BasePacket
    {
        public SetHealthTest(uint netId, short unk, float maxhp, float hp) 
            : base(PacketCmd.PKT_S2C_SetHealth, netId)
        {
            buffer.Write((short)unk); // unk,maybe flags for physical/magical/true dmg
            buffer.Write((float)maxhp);
            buffer.Write((float)hp);
        }
    }
}