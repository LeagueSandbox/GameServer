using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleStartGame : IPacketHandler
    {
        private Game _game = Program.ResolveDependency<Game>();
        private PlayerManager _playerManager = Program.ResolveDependency<PlayerManager>();

        public bool HandlePacket(Peer peer, byte[] data)
        {
            if (_game.IsRunning)
                return true;

            _game.IncrementReadyPlayers();
            if (_game.PlayersReady == _playerManager.GetPlayers().Count)
            {
                var start = new StatePacket(PacketCmdS2C.PKT_S2C_StartGame);
                _game.PacketHandlerManager.broadcastPacket(start, Channel.CHL_S2C);

                foreach (var player in _playerManager.GetPlayers())
                {
                    if (player.Item2.Peer == peer && !player.Item2.IsMatchingVersion)
                    {
                        var dm = new DebugMessage("Your client version does not match the server. Check the server log for more information.");
                        _game.PacketHandlerManager.sendPacket(peer, dm, Channel.CHL_S2C);
                    }
                    _game.PacketNotifier.notifyUpdatedStats(player.Item2.Champion, false);
                }

                _game.Start();
            }

            if (_game.IsRunning)
            {
                foreach (var p in _playerManager.GetPlayers())
                {
                    var map = _game.Map;
                    map.AddObject(p.Item2.Champion);

                    // Send the initial game time sync packets, then let the map send another
                    float gameTime = map.GameTime / 1000.0f;

                    var timer = new GameTimer(gameTime); // 0xC1
                    _game.PacketHandlerManager.sendPacket(p.Item2.Peer, timer, Channel.CHL_S2C);

                    var timer2 = new GameTimerUpdate(gameTime); // 0xC2
                    _game.PacketHandlerManager.sendPacket(p.Item2.Peer, timer2, Channel.CHL_S2C);
                }
            }

            return true;
        }
    }
}
