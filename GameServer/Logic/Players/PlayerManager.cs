using ENet;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets;
using System.Collections.Generic;

namespace LeagueSandbox.GameServer.Logic.Players
{
    public class PlayerManager
    {
        private NetworkIdManager _networkIdManager;

        private List<Pair<uint, ClientInfo>> _players = new List<Pair<uint, ClientInfo>>();
        private int _currentId = 1;

        public PlayerManager(NetworkIdManager networkIdManager)
        {
            _networkIdManager = networkIdManager;
        }

        public void AddPlayer(KeyValuePair<string, PlayerConfig> p)
        {
            SummonerSpellIds[] summonerSkills = new SummonerSpellIds[]
            {
                EnumParser.ParseSummonerSpell(p.Value.Summoner1),
                EnumParser.ParseSummonerSpell(p.Value.Summoner2)
            };
            var player = new ClientInfo(
                p.Value.Rank,
                ((p.Value.Team.ToLower() == "blue") ? TeamId.TEAM_BLUE : TeamId.TEAM_PURPLE),
                p.Value.Ribbon,
                p.Value.Icon,
                p.Value.Skin,
                p.Value.Name,
                summonerSkills,
                _currentId // same as StartClient.bat
            );
            _currentId++;

            var c = new Champion(p.Value.Champion, (uint)player.UserId, p.Value.Runes);
            var pos = c.getRespawnPosition();

            c.setPosition(pos.Item1, pos.Item2);

            if (p.Value.Team.ToLower() == "blue")
            {
                c.SetTeam(TeamId.TEAM_BLUE);
            }
            else
            {
                c.SetTeam(TeamId.TEAM_PURPLE);
            }

            c.LevelUp();

            player.Champion = c;
            var pair = new Pair<uint, ClientInfo>();
            pair.Item2 = player;
            _players.Add(pair);
        }

        // GetPlayerFromPeer
        public ClientInfo GetPeerInfo(Peer peer)
        {
            foreach (var player in _players)
                if (player.Item1 == peer.Address.port)
                    return player.Item2;
            return null;
        }

        public List<Pair<uint, ClientInfo>> GetPlayers()
        {
            return _players;
        }
    }
}
