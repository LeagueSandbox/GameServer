﻿using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
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
        public bool keyChecked = false;
        public bool versionMatch = true;
        public bool Disconnected = false;
        public long userId;
        public int ticks = 0;
        public int skinNo = 0;
        public SummonerSpellIds[] summonerSkills = new SummonerSpellIds[2];
        public string name, rank;
        public short ribbon;
        public int icon;
        public TeamId team;

        Champion champion;
        Peer peer;

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

        public void setPeer(Peer peer)
        {
            this.peer = peer;
        }

        public Peer getPeer()
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
        TEAM_PURPLE = 0xC8,
        TEAM_NEUTRAL = 0x02 // I just raped the enum. Oh well...
    }
}
