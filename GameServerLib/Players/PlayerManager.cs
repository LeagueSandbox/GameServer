using GameServerCore.NetInfo;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Packets;
using System.Collections.Generic;

namespace LeagueSandbox.GameServer.Players
{
    public class PlayerManager
    {
        private NetworkIdManager _networkIdManager;
        private Game _game;

        private List<ClientInfo> _players = new List<ClientInfo>();
        private Dictionary<TeamId, int> _userIdsPerTeam = new Dictionary<TeamId, int>
        {
            { TeamId.TEAM_BLUE, 0 },
            { TeamId.TEAM_PURPLE, 0 }
        };

        public PlayerManager(Game game)
        {
            _game = game;
            _networkIdManager = game.NetworkIdManager;
        }

        public void AddPlayer(PlayerConfig config)
        {
            var summonerSkills = new[]
            {
                config.Summoner1,
                config.Summoner2
            };
            var teamId = config.Team;
            var info = new ClientInfo(
                config.Rank,
                teamId,
                config.Ribbon,
                config.Icon,
                config.Skin,
                config.Name,
                summonerSkills,
                config.PlayerID
            );
            
            info.ClientId = _players.Count;
            _userIdsPerTeam[teamId]++;

            var c = new Champion(
                _game,
                config.Champion,
                config.Runes,
                config.Talents,
                info,
                0,
                teamId
            );

            var pos = c.GetSpawnPosition(_userIdsPerTeam[teamId]);
            c.SetPosition(pos, false);
            c.StopMovement();
            c.UpdateMoveOrder(OrderType.Stop);
            info.Champion = c;
            _players.Add(info);

            _game.ObjectManager.AddObject(c);
        }

        public void AddPlayer(ClientInfo info)
        {
            info.ClientId = _players.Count;
            _players.Add(info);
        }

        // GetPlayerFromPeer
        public ClientInfo GetPeerInfo(int clientId)
        {
            if (0 <= clientId && clientId < _players.Count)
            {
                return _players[clientId];
            }
            return null;
        }

        public ClientInfo GetClientInfoByPlayerId(long playerId)
        {
            return _players.Find(c => c.PlayerId == playerId);
        }

        public ClientInfo GetClientInfoByChampion(Champion champ)
        {
            return _players.Find(c => c.Champion == champ);
        }

        public List<ClientInfo> GetPlayers(bool includeBots = true)
        {
            if (!includeBots)
            {
                return _players.FindAll(c => !c.Champion.IsBot);
            }

            return _players;
        }
    }
}
