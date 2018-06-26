using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
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

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_CharLoaded;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleSpawn(Logger logger, Game game, ItemManager itemManager, PlayerManager playerManager,
            NetworkIdManager networkIdManager)
        {
            _logger = logger;
            _game = game;
            _itemManager = itemManager;
            _playerManager = playerManager;
            _networkIdManager = networkIdManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var start = new StatePacket2(PacketCmd.PKT_S2C_StartSpawn);
            _game.PacketHandlerManager.sendPacket(peer, start, Channel.CHL_S2C);
            _logger.LogCoreInfo("Spawning map");

            int playerId = 0;
            foreach (var p in _playerManager.GetPlayers())
            {
                var spawn = new HeroSpawn(p.Item2, playerId++);
                _game.PacketHandlerManager.sendPacket(peer, spawn, Channel.CHL_S2C);

                var info = new AvatarInfo(p.Item2);
                _game.PacketHandlerManager.sendPacket(peer, info, Channel.CHL_S2C);
            }

            var peerInfo = _playerManager.GetPeerInfo(peer);
            var bluePill = _itemManager.GetItemType(_game.Map.MapGameScript.BluePillId);
            var itemInstance = peerInfo.Champion.getInventory().SetExtraItem(7, bluePill);
            var buyItem = new BuyItemResponse(peerInfo.Champion, itemInstance);
            _game.PacketHandlerManager.sendPacket(peer, buyItem, Channel.CHL_S2C);

            // Runes
            byte runeItemSlot = 14;
            foreach (var rune in peerInfo.Champion.RuneList._runes)
            {
                var runeItem = _itemManager.GetItemType(rune.Value);
                var newRune = peerInfo.Champion.getInventory().SetExtraItem(runeItemSlot, runeItem);
                _playerManager.GetPeerInfo(peer).Champion.Stats.AddModifier(runeItem);
                runeItemSlot++;
            }

            // Not sure why both 7 and 14 skill slot, but it does not seem to work without it
            var skillUp = new SkillUpResponse(peerInfo.Champion.NetId, 7, 1, (byte)peerInfo.Champion.getSkillPoints());
            _game.PacketHandlerManager.sendPacket(peer, skillUp, Channel.CHL_GAMEPLAY);
            skillUp = new SkillUpResponse(peerInfo.Champion.NetId, 14, 1, (byte)peerInfo.Champion.getSkillPoints());
            _game.PacketHandlerManager.sendPacket(peer, skillUp, Channel.CHL_GAMEPLAY);

            peerInfo.Champion.Stats.SetSpellEnabled(7, true);
            peerInfo.Champion.Stats.SetSpellEnabled(14, true);
            peerInfo.Champion.Stats.SetSummonerSpellEnabled(0, true);
            peerInfo.Champion.Stats.SetSummonerSpellEnabled(1, true);

            var objects = _game.ObjectManager.GetObjects();
            foreach (var kv in objects)
            {
                if (kv.Value is LaneTurret)
                {
                    var turret = kv.Value as LaneTurret;
                    var turretSpawn = new TurretSpawn(turret);
                    _game.PacketHandlerManager.sendPacket(peer, turretSpawn, Channel.CHL_S2C);

                    // Fog Of War
                    var fogOfWarPacket = new FogUpdate2(turret, _networkIdManager);
                    _game.PacketHandlerManager.broadcastPacketTeam(turret.Team, fogOfWarPacket, Channel.CHL_S2C);

                    // To suppress game HP-related errors for enemy turrets out of vision
                    var setHealthPacket = new SetHealth(turret);
                    _game.PacketHandlerManager.sendPacket(peer, setHealthPacket, Channel.CHL_S2C);

                    foreach (var item in turret.Inventory)
                    {
                        if (item == null) continue;
                        _game.PacketNotifier.NotifyItemBought(turret, item as Item);
                    }

                    continue;
                }
                else if (kv.Value is LevelProp)
                {
                    var levelProp = kv.Value as LevelProp;

                    var levelPropSpawnPacket = new LevelPropSpawn(levelProp);
                    _game.PacketHandlerManager.sendPacket(peer, levelPropSpawnPacket, Channel.CHL_S2C);

                    continue;
                }
                else if (kv.Value is Champion)
                {
                    var champion = kv.Value as Champion;
                    if (champion.IsVisibleByTeam(peerInfo.Champion.Team))
                    {
                        var enterVisionPacket = new EnterVisionAgain(champion);
                        _game.PacketHandlerManager.sendPacket(peer, enterVisionPacket, Channel.CHL_S2C);
                    }
                }
                else if (kv.Value is Inhibitor || kv.Value is Nexus)
                {
                    var inhibtor = kv.Value as AttackableUnit;

                    var minionSpawnPacket = new MinionSpawn2(inhibtor.NetId);
                    _game.PacketHandlerManager.sendPacket(peer, minionSpawnPacket, Channel.CHL_S2C);
                    var setHealthPacket = new SetHealth(inhibtor.NetId);
                    _game.PacketHandlerManager.sendPacket(peer, setHealthPacket, Channel.CHL_S2C);

                    continue;
                }
                else if (kv.Value is Projectile)
                {
                    var projectile = kv.Value as Projectile;
                    if (projectile.IsVisibleByTeam(peerInfo.Champion.Team))
                    {
                        var spawnProjectilePacket = new SpawnProjectile(projectile);
                        _game.PacketHandlerManager.sendPacket(peer, spawnProjectilePacket, Channel.CHL_S2C);
                    }

                    continue;
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
                var minionSpawnPacket = new MinionSpawn2(0xff10c6db);
                _game.PacketHandlerManager.sendPacket(peer, minionSpawnPacket, Channel.CHL_S2C);
                var setHealthPacket = new SetHealth(0xff10c6db);
                _game.PacketHandlerManager.sendPacket(peer, setHealthPacket, Channel.CHL_S2C);
            }
            else if (peerInfo != null && peerInfo.Team == TeamId.TEAM_PURPLE)
            {
                // Shop (purple team)
                var minionSpawnPacket = new MinionSpawn2(0xffa6170e);
                _game.PacketHandlerManager.sendPacket(peer, minionSpawnPacket, Channel.CHL_S2C);
                var setHealthPacket = new SetHealth(0xffa6170e);
                _game.PacketHandlerManager.sendPacket(peer, setHealthPacket, Channel.CHL_S2C);
            }

            var endSpawnPacket = new StatePacket(PacketCmd.PKT_S2C_EndSpawn);
            return _game.PacketHandlerManager.sendPacket(peer, endSpawnPacket, Channel.CHL_S2C);
        }
    }
}
