using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class ChampionDie : BasePacket
    {
        public ChampionDie(Champion die, Unit killer, int goldFromKill)
            : base(PacketCmd.PKT_S2C_ChampionDie, die.NetId)
        {
            buffer.Write(goldFromKill); // Gold from kill?
            buffer.Write((byte)0);
            if (killer != null)
                buffer.Write(killer.NetId);
            else
                buffer.Write((int)0);

            buffer.Write((byte)0);
            buffer.Write((byte)7);
            buffer.Write(die.RespawnTimer / 1000.0f); // Respawn timer, float
        }
    }
}