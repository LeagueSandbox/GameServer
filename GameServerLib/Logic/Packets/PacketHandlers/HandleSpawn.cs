using ENet;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.Buildings.AnimatedBuildings;
using LeagueSandbox.GameServer.Logic.GameObjects.Missiles;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleSpawn : PacketHandlerBase
    {
        private readonly Logger _logger;
        private readonly Game _game;
        private readonly ItemManager _itemManager;
        private readonly PlayerManager _playerManager;
        private readonly NetworkIdManager _networkIdManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_CHAR_LOADED;
        public override Channel PacketChannel => Channel.CHL_C2_S;

        public HandleSpawn(Game game)
        {
            _logger = game.GetLogger();
            _game = game;
            _itemManager = game.GetItemManager();
            _playerManager = game.GetPlayerManager();
            _networkIdManager = game.GetNetworkManager();
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var start = new StatePacket2(_game, PacketCmd.PKT_S2C_START_SPAWN);
            _game.PacketHandlerManager.SendPacket(peer, start, Channel.CHL_S2_C);
            _logger.LogCoreInfo("Spawning map");

            var playerId = 0;
            foreach (var p in _playerManager.GetPlayers())
            {
                var spawn = new HeroSpawn(_game, p.Item2, playerId++);
                _game.PacketHandlerManager.SendPacket(peer, spawn, Channel.CHL_S2_C);

                var info = new AvatarInfo(_game, p.Item2);
                _game.PacketHandlerManager.SendPacket(peer, info, Channel.CHL_S2_C);
            }

            var peerInfo = _playerManager.GetPeerInfo(peer);
            var bluePill = _itemManager.GetItemType(_game.Map.MapGameScript.BluePillId);
            var itemInstance = peerInfo.Champion.GetInventory().SetExtraItem(7, bluePill);
            var buyItem = new BuyItemResponse(_game, peerInfo.Champion, itemInstance);
            _game.PacketHandlerManager.SendPacket(peer, buyItem, Channel.CHL_S2_C);

            // Runes
            byte runeItemSlot = 14;
            foreach (var rune in peerInfo.Champion.RuneList.Runes)
            {
                var runeItem = _itemManager.GetItemType(rune.Value);
                var newRune = peerInfo.Champion.GetInventory().SetExtraItem(runeItemSlot, runeItem);
                _playerManager.GetPeerInfo(peer).Champion.Stats.AddModifier(runeItem);
                runeItemSlot++;
            }

            // Not sure why both 7 and 14 skill slot, but it does not seem to work without it
            var skillUp = new SkillUpResponse(_game, peerInfo.Champion.NetId, 7, 1, (byte)peerInfo.Champion.GetSkillPoints());
            _game.PacketHandlerManager.SendPacket(peer, skillUp, Channel.CHL_GAMEPLAY);
            skillUp = new SkillUpResponse(_game, peerInfo.Champion.NetId, 14, 1, (byte)peerInfo.Champion.GetSkillPoints());
            _game.PacketHandlerManager.SendPacket(peer, skillUp, Channel.CHL_GAMEPLAY);

            peerInfo.Champion.Stats.SetSpellEnabled(7, true);
            peerInfo.Champion.Stats.SetSpellEnabled(14, true);
            peerInfo.Champion.Stats.SetSummonerSpellEnabled(0, true);
            peerInfo.Champion.Stats.SetSummonerSpellEnabled(1, true);

            var objects = _game.ObjectManager.GetObjects();
            foreach (var kv in objects)
            {
                if (kv.Value is LaneTurret turret)
                {
                    var turretSpawn = new TurretSpawn(_game, turret);
                    _game.PacketHandlerManager.SendPacket(peer, turretSpawn, Channel.CHL_S2_C);

                    // Fog Of War
                    var fogOfWarPacket = new FogUpdate2(_game, turret, _networkIdManager);
                    _game.PacketHandlerManager.BroadcastPacketTeam(turret.Team, fogOfWarPacket, Channel.CHL_S2_C);

                    // To suppress game HP-related errors for enemy turrets out of vision
                    var setHealthPacket = new SetHealth(_game, turret);
                    _game.PacketHandlerManager.SendPacket(peer, setHealthPacket, Channel.CHL_S2_C);

                    foreach (var item in turret.Inventory)
                    {
                        if (item == null) continue;
                        _game.PacketNotifier.NotifyItemBought(turret, item as Item);
                    }
                }
                else if (kv.Value is LevelProp levelProp)
                {
                    var levelPropSpawnPacket = new LevelPropSpawn(_game, levelProp);
                    _game.PacketHandlerManager.SendPacket(peer, levelPropSpawnPacket, Channel.CHL_S2_C);
                }
                else if (kv.Value is Champion champion)
                {
                    if (champion.IsVisibleByTeam(peerInfo.Champion.Team))
                    {
                        var enterVisionPacket = new EnterVisionAgain(_game, champion);
                        _game.PacketHandlerManager.SendPacket(peer, enterVisionPacket, Channel.CHL_S2_C);
                    }
                }
                else if (kv.Value is Inhibitor || kv.Value is Nexus)
                {
                    var inhibtor = (AttackableUnit) kv.Value;

                    var minionSpawnPacket = new MinionSpawn2(_game, inhibtor.NetId);
                    _game.PacketHandlerManager.SendPacket(peer, minionSpawnPacket, Channel.CHL_S2_C);
                    var setHealthPacket = new SetHealth(_game, inhibtor.NetId);
                    _game.PacketHandlerManager.SendPacket(peer, setHealthPacket, Channel.CHL_S2_C);
                }
                else if (kv.Value is Projectile projectile)
                {
                    if (projectile.IsVisibleByTeam(peerInfo.Champion.Team))
                    {
                        var spawnProjectilePacket = new SpawnProjectile(_game, projectile);
                        _game.PacketHandlerManager.SendPacket(peer, spawnProjectilePacket, Channel.CHL_S2_C);
                    }
                }
                else
                {
                    _logger.LogCoreWarning("Object of type: " + kv.Value.GetType() + " not handled in HandleSpawn.");
                }
            }

            // TODO shop map specific?
            // Level props are just models, we need button-object minions to allow the client to interact with it
            if (peerInfo != null && peerInfo.Team == TeamId.TEAM_BLUE)
            {
                // Shop (blue team)
                var minionSpawnPacket = new MinionSpawn2(_game, 0xff10c6db);
                _game.PacketHandlerManager.SendPacket(peer, minionSpawnPacket, Channel.CHL_S2_C);
                var setHealthPacket = new SetHealth(_game, 0xff10c6db);
                _game.PacketHandlerManager.SendPacket(peer, setHealthPacket, Channel.CHL_S2_C);
            }
            else if (peerInfo != null && peerInfo.Team == TeamId.TEAM_PURPLE)
            {
                // Shop (purple team)
                var minionSpawnPacket = new MinionSpawn2(_game, 0xffa6170e);
                _game.PacketHandlerManager.SendPacket(peer, minionSpawnPacket, Channel.CHL_S2_C);
                var setHealthPacket = new SetHealth(_game, 0xffa6170e);
                _game.PacketHandlerManager.SendPacket(peer, setHealthPacket, Channel.CHL_S2_C);
            }

            var endSpawnPacket = new StatePacket(_game, PacketCmd.PKT_S2C_END_SPAWN);
            return _game.PacketHandlerManager.SendPacket(peer, endSpawnPacket, Channel.CHL_S2_C);
        }
    }
}
