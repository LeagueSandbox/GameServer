using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Logic.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;

namespace LeagueSandbox.GameServer.Logic.Enet
{
    public class ClientInfo
    {
        public long UserId { get; set; }
        private bool VersionMatch = true;
        private int SkinNo = 0;
        private SummonerSpellIds[] SummonerSkills = new SummonerSpellIds[2];
        private string Name, Rank;
        private short Ribbon;
        private int Icon;
        private TeamId Team;

        private Champion _champion;
        private Peer _peer;

        public ClientInfo(string rank, TeamId team, short ribbon, int icon)
        {
            Rank = rank;
            Team = team;
            Ribbon = ribbon;
            Icon = icon;
        }

        public void SetName(string name)
        {
            this.Name = name;
        }

        public void SetChampion(Champion champion)
        {
            this._champion = champion; //ChampionFactory::getChampionFromType(type);
            champion.setSkin(SkinNo);
        }

        public void SetSkinNo(short skinNo)
        {
            this.SkinNo = skinNo;
        }

        public string GetRank()
        {
            return Rank;
        }
        public TeamId GetTeam()
        {
            return Team;
        }

        public Champion GetChampion()
        {
            return _champion;
        }

        public string GetName()
        {
            return Name;
        }

        public int GetSkinNo()
        {
            return SkinNo;
        }

        public void SetSummoners(SummonerSpellIds sum1, SummonerSpellIds sum2)
        {
            SummonerSkills[0] = sum1;
            SummonerSkills[1] = sum2;
        }

        public SummonerSpellIds[] getSummoners()
        {
            return SummonerSkills;
        }

        public int GetRibbon()
        {
            return Ribbon;
        }

        public int GetIcon()
        {
            return Icon;
        }

        public void SetPeer(Peer peer)
        {
            this._peer = peer;
        }

        public Peer GetPeer()
        {
            return _peer;
        }

        public void SetVersionMatch(bool versionMatch)
        {
            this.VersionMatch = versionMatch;
        }

        public bool IsVersionMatch()
        {
            return VersionMatch;
        }
    }
    public enum TeamId : int
    {
        TEAM_BLUE = 0x64,
        TEAM_PURPLE = 0xC8,
        TEAM_NEUTRAL = 0x12C
    }
}
