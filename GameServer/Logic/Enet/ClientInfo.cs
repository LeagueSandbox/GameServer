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
        public bool KeyChecked = false;
        public bool VersionMatch = true;
        public long UserId;
        public int SkinNo = 0;
        public SummonerSpellIds[] SummonerSkills = new SummonerSpellIds[2];
        public string Name, Rank;
        public short Ribbon;
        public int Icon;
        public TeamId Team;

        private Champion _champion;
        private Peer _peer;

        public ClientInfo(string rank, TeamId team, short ribbon, int icon)
        {
            this.Rank = rank;
            this.Team = team;
            this.Ribbon = ribbon;
            this.Icon = icon;
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
        TEAM_NEUTRAL = 0x02 // I just raped the enum. Oh well...
    }
}
