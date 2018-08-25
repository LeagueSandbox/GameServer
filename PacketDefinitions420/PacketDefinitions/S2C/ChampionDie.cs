using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class ChampionDie : BasePacket
    {
        public ChampionDie(IChampion die, IAttackableUnit killer, int goldFromKill)
            : base(PacketCmd.PKT_S2C_CHAMPION_DIE, die.NetId)
        {
            Write((int)goldFromKill); // Gold from kill?
            Write((byte)0);
            WriteNetId(killer);

            Write((byte)0);
            Write((byte)7);
            Write(die.RespawnTimer / 1000.0f); // Respawn timer, float
        }
    }
}