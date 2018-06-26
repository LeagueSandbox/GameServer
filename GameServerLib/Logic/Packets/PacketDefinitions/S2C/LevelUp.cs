using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class LevelUp : BasePacket
    {
        public LevelUp(Champion c)
            : base(PacketCmd.PKT_S2_C_LEVEL_UP, c.NetId)
        {
            _buffer.Write(c.Stats.Level);
            _buffer.Write(c.GetSkillPoints());
        }
    }
}