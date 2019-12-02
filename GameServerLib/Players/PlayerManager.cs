﻿using GameServerCore;
using GameServerCore.Domain.GameObjects;
using GameServerCore.NetInfo;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Packets;
using System.Collections.Generic;
using System;

namespace LeagueSandbox.GameServer.Players
{
    public class PlayerManager : IPlayerManager
    {
        private NetworkIdManager _networkIdManager;
        private Game _game;

        private List<Tuple<uint, ClientInfo>> _players = new List<Tuple<uint, ClientInfo>>();
        private ulong _currentId = 1;
        private Dictionary<TeamId, uint> _userIdsPerTeam = new Dictionary<TeamId, uint>
        {
            { TeamId.TEAM_BLUE, 0 },
            { TeamId.TEAM_PURPLE, 0 }
        };

        public PlayerManager(Game game)
        {
            _game = game;
            _networkIdManager = game.NetworkIdManager;
        }

        private TeamId GetTeamIdFromConfig(PlayerConfig p)
        {
            if (p.Team.ToLower().Equals("blue"))
            {
                return TeamId.TEAM_BLUE;
            }

            return TeamId.TEAM_PURPLE;
        }

        public void AddPlayer(KeyValuePair<string, PlayerConfig> p)
        {
            var summonerSkills = new[]
            {
                p.Value.Summoner1,
                p.Value.Summoner2
            };
            var teamId = GetTeamIdFromConfig(p.Value);
            var player = new ClientInfo(
                p.Value.Rank,
                teamId,
                p.Value.Ribbon,
                p.Value.Icon,
                p.Value.Skin,
                p.Value.Name,
                summonerSkills,
                _currentId // same as StartClient.bat
            );
            _currentId++;
            var c = new Champion(_game, p.Value.Champion, (uint)player.PlayerId, _userIdsPerTeam[teamId]++, p.Value.Runes, player);
            c.SetTeam(teamId);

            var pos = c.GetSpawnPosition();
            c.SetPosition(pos.X, pos.Y);
            c.LevelUp();

            player.Champion = c;
            var pair = new Tuple<uint, ClientInfo> ((uint)player.PlayerId, player);
            _players.Add(pair);
        }

        // GetPlayerFromPeer
        public ClientInfo GetPeerInfo(ulong playerId)
        {
            foreach (var player in _players)
            {
                if (player.Item2.PlayerId == playerId)
                {
                    return player.Item2;
                }
            }

            return null;
        }

        public ClientInfo GetClientInfoByChampion(IChampion champ)
        {
            return GetPlayers().Find(c => c.Item2.Champion == champ).Item2;
        }

        public List<Tuple<uint, ClientInfo>> GetPlayers()
        {
            return _players;
        }
    }
}
