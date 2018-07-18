using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SetHealthTest : BasePacket
    {
        public SetHealthTest(Game game, uint netId, short unk, float maxhp, float hp)
            : base(game, PacketCmd.PKT_S2C_SET_HEALTH, netId)
        {
            Write(unk); // unk,maybe flags for physical/magical/true dmg
            Write(maxhp);
            Write(hp);
        }
    }
}