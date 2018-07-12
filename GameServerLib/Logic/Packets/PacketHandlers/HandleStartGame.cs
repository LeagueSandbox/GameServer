using ENet;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleStartGame : PacketHandlerBase
    {

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_STARTGame;
        public override Channel PacketChannel => Channel.CHL_C2_S;

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var peerInfo = PlayerManager.GetPeerInfo(peer);

            if (!peerInfo.IsDisconnected)
            {
                Game.IncrementReadyPlayers();
            }

            /* if (Game.IsRunning)
                return true; */

            if (Game.PlayersReady == PlayerManager.GetPlayers().Count)
            {
                var start = new StatePacket(PacketCmd.PKT_S2C_STARTGame);
                Game.PacketHandlerManager.BroadcastPacket(start, Channel.CHL_S2_C);

                foreach (var player in PlayerManager.GetPlayers())
                {
                    if (player.Item2.Peer == peer && !player.Item2.IsMatchingVersion)
                    {
                        var msg = "Your client version does not match the server. " +
                                  "Check the server log for more information.";
                        var dm = new DebugMessage(msg);
                        Game.PacketHandlerManager.SendPacket(peer, dm, Channel.CHL_S2_C);
                    }
                    Game.PacketNotifier.NotifySetHealth(player.Item2.Champion);
                    // TODO: send this in one place only
                    Game.PacketNotifier.NotifyUpdatedStats(player.Item2.Champion, false);
                }

                Game.Start();
            }

            if (Game.IsRunning)
            {
                if (peerInfo.IsDisconnected)
                {
                    foreach (var player in PlayerManager.GetPlayers())
                    {
                        if (player.Item2.Team == peerInfo.Team)
                        {
                            var heroSpawnPacket = new HeroSpawn2(player.Item2.Champion);
                            Game.PacketHandlerManager.SendPacket(peer, heroSpawnPacket, Channel.CHL_S2_C);

                            /* This is probably not the best way
                             * of updating a champion's level, but it works */
                            var levelUpPacket = new LevelUp(player.Item2.Champion);
                            Game.PacketHandlerManager.SendPacket(peer, levelUpPacket, Channel.CHL_S2_C);
                            if (Game.IsPaused)
                            {
                                var pausePacket = new PauseGame((int)Game.PauseTimeLeft, true);
                                Game.PacketHandlerManager.SendPacket(peer, pausePacket, Channel.CHL_S2_C);
                            }
                        }
                    }
                    peerInfo.IsDisconnected = false;
                    Game.PacketNotifier.NotifyUnitAnnounceEvent(UnitAnnounces.SUMMONER_RECONNECTED, peerInfo.Champion);

                    // Send the initial game time sync packets, then let the map send another
                    var gameTime = Game.GameTime / 1000.0f;

                    var timer = new GameTimer(gameTime); // 0xC1
                    Game.PacketHandlerManager.SendPacket(peer, timer, Channel.CHL_S2_C);

                    var timer2 = new GameTimerUpdate(gameTime); // 0xC2
                    Game.PacketHandlerManager.SendPacket(peer, timer2, Channel.CHL_S2_C);

                    return true;
                }

                foreach (var p in PlayerManager.GetPlayers())
                {
                    Game.ObjectManager.AddObject(p.Item2.Champion);

                    // Send the initial game time sync packets, then let the map send another
                    var gameTime = Game.GameTime / 1000.0f;

                    var timer = new GameTimer(gameTime); // 0xC1
                    Game.PacketHandlerManager.SendPacket(p.Item2.Peer, timer, Channel.CHL_S2_C);

                    var timer2 = new GameTimerUpdate(gameTime); // 0xC2
                    Game.PacketHandlerManager.SendPacket(p.Item2.Peer, timer2, Channel.CHL_S2_C);
                }
            }

            return true;
        }
    }
}
