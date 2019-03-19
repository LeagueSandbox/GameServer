using GameServerCore.Domain.GameObjects;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class ChampionDieResponse : ICoreResponse
    {
        public IChampion Die { get; }
        public IAttackableUnit Killer { get; }
        public int GoldFromKill { get; }
        public ChampionDieResponse(IChampion die, IAttackableUnit killer, int goldFromKill)
        {
            Die = die;
            Killer = killer;
            GoldFromKill = goldFromKill;
        }
    }
};