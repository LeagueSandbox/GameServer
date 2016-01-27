using static ENet.Native;
using IntWarsSharp.Core.Logic.PacketHandlers;
using IntWarsSharp.Logic.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntWarsSharp.Logic.Enet
{
    public unsafe class ClientInfo
    {
        public bool keyChecked = false;
        public bool versionMatch = true;
        public int userId;
        public int ticks = 0;
        public int skinNo = 0;
        public SummonerSpellIds[] summonerSkills = new SummonerSpellIds[2];
        public string name, rank;
        public short ribbon;
        public int icon;
        public TeamId team;

        Champion champion;
        ENetPeer* peer;

        public ClientInfo(string rank, TeamId team, short ribbon, int icon)
        {
            this.rank = rank;
            this.team = team;
            this.ribbon = ribbon;
            this.icon = icon;
        }


        public void setName(string name)
        {
            this.name = name;
        }

        public void setChampion(Champion champion)
        {
            this.champion = champion; //ChampionFactory::getChampionFromType(type);
            champion.setSkin(skinNo);
        }

        public void setSkinNo(short skinNo)
        {
            this.skinNo = skinNo;
        }

        public string getRank()
        {
            return rank;
        }
        public TeamId getTeam()
        {
            return team;
        }

        public Champion getChampion()
        {
            return champion;
        }

        public string getName()
        {
            return name;
        }

        public int getSkinNo()
        {
            return skinNo;
        }

        public void setSummoners(SummonerSpellIds sum1, SummonerSpellIds sum2)
        {
            summonerSkills[0] = sum1;
            summonerSkills[1] = sum2;
        }

        public void setTicks(int a)
        {
            ticks = a;
        }

        public int getRibbon()
        {
            return ribbon;
        }

        public int getIcon()
        {
            return icon;
        }

        public int getTicks()
        {
            ticks++;
            return ticks;
        }

        public void setPeer(ENetPeer* peer)
        {
            this.peer = peer;
        }

        public ENetPeer* getPeer()
        {
            return peer;
        }

        public void setVersionMatch(bool versionMatch)
        {
            this.versionMatch = versionMatch;
        }

        public bool isVersionMatch()
        {
            return versionMatch;
        }
    }
    public enum TeamId : int
    {
        TEAM_BLUE = 0x64,
        TEAM_PURPLE = 0xC8
    };
}
