using GameServerCore.Packets.PacketDefinitions.Requests;
using GameServerCore;
using GameServerCore.Packets.Handlers;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleJoinTeam : PacketHandlerBase<JoinTeamRequest>
    {
        private readonly Game _game;
        private readonly IPlayerManager _playerManager;

        public HandleJoinTeam(Game game)
        {
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, JoinTeamRequest req)
        {
            var players = _playerManager.GetPlayers(true);
            uint version = uint.Parse(Config.VERSION.ToString().Replace(".", string.Empty));

            // Builds team info e.g. first UserId set on Blue has PlayerId 0
            // increment by 1 for each added player
            _game.PacketNotifier.NotifyLoadScreenInfo(userId, players);

            // Distributes each players info by UserId
            foreach (var player in players)
            {
                // Load everyone's player name.
                _game.PacketNotifier.NotifyRequestRename(userId, player);
                // Load everyone's champion.
                _game.PacketNotifier.NotifyRequestReskin(userId, player);

                // Let other players know we loaded in.
                if (player.Item1 == userId)
                {
                    _game.PacketNotifier.NotifyKeyCheck((int)player.Item2.ClientId, player.Item2.PlayerId, version, broadcast: true);
                }
            }

            return true;
        }
    }
}