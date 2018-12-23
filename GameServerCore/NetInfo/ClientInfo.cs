﻿using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;

namespace GameServerCore.NetInfo
{
    public class ClientInfo
    {
        public ulong PlayerId { get; private set; }
        public uint ClientId { get; set; }
        public bool IsMatchingVersion { get; set; }
        public bool IsDisconnected { get; set; }
        public bool IsStartedClient { get; set; }
        public int SkinNo { get; private set; }
        public string[] SummonerSkills { get; private set; }
        public string Name { get; private set; }
        public string Rank { get; private set; }
        public short Ribbon { get; private set; }
        public int Icon { get; private set; }
        public TeamId Team { get; private set; }

        private IChampion _champion;
        public IChampion Champion
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
                          ulong playerId)
        {
            Rank = rank;
            Team = team;
            Ribbon = ribbon;
            Icon = icon;
            SkinNo = skinNo;
            IsMatchingVersion = true;
            IsDisconnected = false;
            Name = name;
            SummonerSkills = summonerSkills;
            PlayerId = playerId;
            IsStartedClient = false;
        }
    }
}
