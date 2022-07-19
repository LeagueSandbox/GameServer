using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;

namespace GameServerCore.NetInfo
{
    public class ClientInfo
    {
        public long PlayerId { get; private set; }
        public int ClientId { get; set; }
        public bool IsMatchingVersion { get; set; }
        /// <summary>
        /// False if the client sent a StartGame request.
        /// </summary>
        public bool IsDisconnected { get; set; }
        /// <summary>
        /// True if the client sent a Handshake request.
        /// </summary>
        public bool IsStartedClient { get; set; }
        public int SkinNo { get; private set; }
        public string[] SummonerSkills { get; private set; }
        public string Name { get; private set; }
        public string Rank { get; private set; }
        public short Ribbon { get; private set; }
        public int Icon { get; private set; }
        public TeamId Team { get; private set; }

        private Champion _champion;
        public Champion Champion
        {
            get => _champion;
            set
            {
                _champion = value;
                _champion.UpdateSkin(SkinNo);
            }
        }

        public ClientInfo(string rank,
                          TeamId team,
                          short ribbon,
                          int icon,
                          int skinNo,
                          string name,
                          string[] summonerSkills,
                          long playerId)
        {
            Rank = rank;
            Team = team;
            Ribbon = ribbon;
            Icon = icon;
            SkinNo = skinNo;
            IsMatchingVersion = true;
            Name = name;
            SummonerSkills = summonerSkills;
            PlayerId = playerId;
            IsDisconnected = true;
            IsStartedClient = false;
        }
    }
}
