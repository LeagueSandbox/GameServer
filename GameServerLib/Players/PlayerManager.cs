using GameServerCore;
using GameServerCore.Domain.GameObjects;
using GameServerCore.NetInfo;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Packets;
using System.Collections.Generic;
using System;
using GameServerCore.Domain;

namespace LeagueSandbox.GameServer.Players
{
    public class PlayerManager : IPlayerManager
    {
        private NetworkIdManager _networkIdManager;
        private Game _game;

        private List<Tuple<uint, ClientInfo>> _players = new List<Tuple<uint, ClientInfo>>();
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

        private TeamId GetTeamIdFromConfig(IPlayerConfig p)
        {
            if (p.Team.ToLower().Equals("blue"))
            {
                return TeamId.TEAM_BLUE;
            }

            return TeamId.TEAM_PURPLE;
        }

        public void AddPlayer(KeyValuePair<string, IPlayerConfig> p)
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
                p.Value.PlayerID // same as StartClient.bat
            );
            
            player.ClientId = (uint)_players.Count;

            var c = new Champion(_game, p.Value.Champion, (uint)player.PlayerId, _userIdsPerTeam[teamId]++, p.Value.Runes, p.Value.Talents, player, 0, teamId);

            var pos = c.GetSpawnPosition((int)_userIdsPerTeam[teamId]);
            c.SetPosition(pos, false);
            c.StopMovement();
            c.UpdateMoveOrder(OrderType.Stop);

            player.Champion = c;
            var pair = new Tuple<uint, ClientInfo> ((uint)player.PlayerId, player);
            _players.Add(pair);
        }

        public void AddPlayer(Tuple<uint, ClientInfo> p)
        {
            _players.Add(p);
        }

        // GetPlayerFromPeer
        public ClientInfo GetPeerInfo(long playerId)
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
            return GetPlayers(true).Find(c => c.Item2.Champion == champ).Item2;
        }

        public List<Tuple<uint, ClientInfo>> GetPlayers(bool includeBots = true)
        {
            if (!includeBots)
            {
                return _players.FindAll(c => !c.Item2.Champion.IsBot);
            }

            return _players;
        }
    }
}
