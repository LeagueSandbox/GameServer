using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class ChampionDie2 : BasePacket
    {
        public ChampionDie2(Champion die, float deathTimer) : base(PacketCmd.PKT_S2C_ChampionDie, die.NetId)
        {
            // Not sure what the whole purpose of that packet is
            buffer.Write(deathTimer);
        }
    }
}