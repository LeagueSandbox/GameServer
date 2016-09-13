using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Logic.GameObjects;
using ENet;

namespace LeagueSandbox.GameServer.Logic.Enet
{
    public class ClientInfo
    {
        public long UserId { get; private set; }
        public bool IsMatchingVersion { get; set; }
        public int SkinNo { get; private set; }
        public SummonerSpellIds[] SummonerSkills { get; private set; }
        public string Name { get; private set; }
        public string  Rank { get; private set; }
        public short Ribbon { get; private set; }
        public int Icon { get; private set; }
        public TeamId Team { get; private set; }

        private Champion _champion;
        public Champion Champion
        {
            get { return _champion; }
            set
            {
                this._champion = value;
                _champion.Skin = SkinNo;
            }
        }

        public Peer Peer { get; set; }

        public ClientInfo(string rank,
                          TeamId team,
                          short ribbon,
                          int icon,
                          int skinNo,
                          string name,
                          SummonerSpellIds[] summonerSkills,
                          long userId)
        {
            Rank = rank;
            Team = team;
            Ribbon = ribbon;
            Icon = icon;
            SkinNo = skinNo;
            IsMatchingVersion = true;
            Name = name;
            SummonerSkills = summonerSkills;
            UserId = userId;
        }
    }
    public enum TeamId : int
    {
        TEAM_BLUE = 0x64,
        TEAM_PURPLE = 0xC8,
        TEAM_NEUTRAL = 0x12C
    }
}
