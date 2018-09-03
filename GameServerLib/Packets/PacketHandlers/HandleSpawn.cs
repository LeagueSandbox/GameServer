using ENet;
using GameServerCore.Enums;
using GameServerCore.Packets.Enums;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings.AnimatedBuildings;
using LeagueSandbox.GameServer.GameObjects.Missiles;
using LeagueSandbox.GameServer.Logging;
using LeagueSandbox.GameServer.Players;
using log4net;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleSpawn : PacketHandlerBase
    {
        private readonly ILog _logger;
        private readonly Game _game;
        private readonly ItemManager _itemManager;
        private readonly PlayerManager _playerManager;
        private readonly NetworkIdManager _networkIdManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_CHAR_LOADED;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleSpawn(Game game)
        {
            _logger = LoggerProvider.GetLogger();
            _game = game;
            _itemManager = game.ItemManager;
            _playerManager = game.PlayerManager;
            _networkIdManager = game.NetworkIdManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
             _game.PacketNotifier.NotifySpawnStart(peer);
            _logger.Debug("Spawning map");

            var playerId = 0;
            foreach (var p in _playerManager.GetPlayers())
            {
                 _game.PacketNotifier.NotifyHeroSpawn(peer, p.Item2, playerId++);
                 _game.PacketNotifier.NotifyAvatarInfo(peer, p.Item2);
            }

            var peerInfo = _playerManager.GetPeerInfo(peer);
            var bluePill = _itemManager.GetItemType(_game.Map.MapGameScript.BluePillId);
            var itemInstance = peerInfo.Champion.Inventory.SetExtraItem(7, bluePill);

             _game.PacketNotifier.NotifyBuyItem(peer, peerInfo.Champion, itemInstance);

            // Runes
            byte runeItemSlot = 14;
            foreach (var rune in peerInfo.Champion.RuneList.Runes)
            {
                var runeItem = _itemManager.GetItemType(rune.Value);
                var newRune = peerInfo.Champion.Inventory.SetExtraItem(runeItemSlot, runeItem);
                _playerManager.GetPeerInfo(peer).Champion.Stats.AddModifier(runeItem);
                runeItemSlot++;
            }

            // Not sure why both 7 and 14 skill slot, but it does not seem to work without it
             _game.PacketNotifier.NotifySkillUp(peer, peerInfo.Champion.NetId, 7, 1, peerInfo.Champion.SkillPoints);
             _game.PacketNotifier.NotifySkillUp(peer, peerInfo.Champion.NetId, 14, 1, peerInfo.Champion.SkillPoints);

            peerInfo.Champion.Stats.SetSpellEnabled(7, true);
            peerInfo.Champion.Stats.SetSpellEnabled(14, true);
            peerInfo.Champion.Stats.SetSummonerSpellEnabled(0, true);
            peerInfo.Champion.Stats.SetSummonerSpellEnabled(1, true);

            var objects = _game.ObjectManager.GetObjects();
            foreach (var kv in objects)
            {
                if (kv.Value is LaneTurret turret)
                {
                     _game.PacketNotifier.NotifyTurretSpawn(peer, turret);

                    // Fog Of War
                     _game.PacketNotifier.NotifyFogUpdate2(turret, _networkIdManager.GetNewNetId());

                    // To suppress game HP-related errors for enemy turrets out of vision
                     _game.PacketNotifier.NotifySetHealth(peer, turret);

                    foreach (var item in turret.Inventory)
                    {
                        if (item == null) continue;
                         _game.PacketNotifier.NotifyItemBought(turret, item as Item);
                    }
                }
                else if (kv.Value is LevelProp levelProp)
                {
                     _game.PacketNotifier.NotifyLevelPropSpawn(peer, levelProp);
                }
                else if (kv.Value is Champion champion)
                {
                    if (champion.IsVisibleByTeam(peerInfo.Champion.Team))
                    {
                         _game.PacketNotifier.NotifyEnterVision(peer, champion);
                    }
                }
                else if (kv.Value is Inhibitor || kv.Value is Nexus)
                {
                    var inhibtor = (AttackableUnit)kv.Value;
                     _game.PacketNotifier.NotifyStaticObjectSpawn(peer, inhibtor.NetId);
                     _game.PacketNotifier.NotifySetHealth(peer, inhibtor.NetId);
                }
                else if (kv.Value is Projectile projectile)
                {
                    if (projectile.IsVisibleByTeam(peerInfo.Champion.Team))
                    {
                         _game.PacketNotifier.NotifyProjectileSpawn(peer, projectile);
                    }
                }
                else
                {
                    _logger.Warn("Object of type: " + kv.Value.GetType() + " not handled in HandleSpawn.");
                }
            }

            // TODO shop map specific?
            // Level props are just models, we need button-object minions to allow the client to interact with it
            if (peerInfo != null && peerInfo.Team == TeamId.TEAM_BLUE)
            {
                // Shop (blue team)
                 _game.PacketNotifier.NotifyStaticObjectSpawn(peer, 0xff10c6db);
                 _game.PacketNotifier.NotifySetHealth(peer, 0xff10c6db);
            }
            else if (peerInfo != null && peerInfo.Team == TeamId.TEAM_PURPLE)
            {
                // Shop (purple team)
                 _game.PacketNotifier.NotifyStaticObjectSpawn(peer, 0xffa6170e);
                 _game.PacketNotifier.NotifySetHealth(peer, 0xffa6170e);
            }

             _game.PacketNotifier.NotifySpawnEnd(peer);
            return true;
        }
    }
}
