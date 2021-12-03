namespace GameServerCore.Domain
{
    public interface IGameFeatures
    {
        bool CooldownsEnabled { get; set; }
        bool ManaCostsEnabled { get; set; }
        bool MinionSpawnsEnabled { get; set; }
    }
}