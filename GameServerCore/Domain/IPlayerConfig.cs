using GameServerCore.Enums;

namespace GameServerCore.Domain
{
    public interface IPlayerConfig
    {
        long PlayerID { get; }
        string Rank { get; }
        string Name { get; }
        string Champion { get; }
        TeamId Team { get; }
        short Skin { get; }
        string Summoner1 { get; }
        string Summoner2 { get; }
        short Ribbon { get; }
        int Icon { get; }
        string BlowfishKey { get; }
        IRuneCollection Runes { get; }
        ITalentInventory Talents { get; }
    }
}