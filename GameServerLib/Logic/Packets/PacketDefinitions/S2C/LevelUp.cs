using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class LevelUp : BasePacket
    {
        public LevelUp(Champion c)
            : base(PacketCmd.PKT_S2C_LevelUp, c.NetId)
        {
            buffer.Write(c.Stats.Level);
            buffer.Write(c.getSkillPoints());
        }
    }
}