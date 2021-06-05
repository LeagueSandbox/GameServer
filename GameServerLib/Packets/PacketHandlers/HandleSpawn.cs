using GameServerCore;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Enums;
using GameServerCore.Packets.Handlers;
using GameServerCore.Packets.PacketDefinitions.Requests;
using LeagueSandbox.GameServer.Items;
using LeagueSandbox.GameServer.Logging;
using log4net;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleSpawn : PacketHandlerBase<SpawnRequest>
    {
        private readonly ILog _logger;
        private readonly Game _game;
        private readonly ItemManager _itemManager;
        private readonly IPlayerManager _playerManager;
        private readonly NetworkIdManager _networkIdManager;

        public HandleSpawn(Game game)
        {
            _logger = LoggerProvider.GetLogger();
            _game = game;
            _itemManager = game.ItemManager;
            _playerManager = game.PlayerManager;
            _networkIdManager = game.NetworkIdManager;
        }

        public override bool HandlePacket(int userId, SpawnRequest req)
        {
             _game.PacketNotifier.NotifyS2C_StartSpawn(userId);
            _logger.Debug("Spawning map");

            var peerInfo = _playerManager.GetPeerInfo(userId);
            var bluePill = _itemManager.GetItemType(_game.Map.MapProperties.BluePillId);
            var itemInstance = peerInfo.Champion.Inventory.SetExtraItem(7, bluePill);

            // self-inform
            _game.PacketNotifier.NotifyS2C_CreateHero(userId, peerInfo);
            _game.PacketNotifier.NotifyAvatarInfo(userId, peerInfo);

            _game.PacketNotifier.NotifyBuyItem(userId, peerInfo.Champion, itemInstance);

            // Runes
            byte runeItemSlot = 14;
            foreach (var rune in peerInfo.Champion.RuneList.Runes)
            {
                var runeItem = _itemManager.GetItemType(rune.Value);
                var newRune = peerInfo.Champion.Inventory.SetExtraItem(runeItemSlot, runeItem);
                _playerManager.GetPeerInfo(userId).Champion.Stats.AddModifier(runeItem);
                runeItemSlot++;
            }

            // Does not seem to work without Recall being skilled and enabled.
            // TODO: Verify if ^ is still true! Commenting it out does not seem to cause any damage.
            _game.PacketNotifier.NotifyNPC_UpgradeSpellAns(userId, peerInfo.Champion.NetId, 13, 1, peerInfo.Champion.SkillPoints);

            peerInfo.Champion.Stats.SetSpellEnabled(13, true);
            peerInfo.Champion.Stats.SetSummonerSpellEnabled(0, true);
            peerInfo.Champion.Stats.SetSummonerSpellEnabled(1, true);

            var objects = _game.ObjectManager.GetObjects();
            foreach (var kv in objects)
            {
                if (kv.Value is IChampion champion)
                {
                    if (champion.IsVisibleByTeam(peerInfo.Champion.Team))
                    {
                        _game.PacketNotifier.NotifyEnterVisibilityClient(champion, userId, true);
                    }
                }
                else if (kv.Value is ILaneTurret turret)
                {
                     _game.PacketNotifier.NotifyTurretSpawn(userId, turret);

                    // TODO: Implemnent a Region class so we can have a centralized (and cleaner) way of making new regions.
                    // Turret Vision (values based on packet data, placeholders)
                    _game.PacketNotifier.NotifyAddRegion
                    (
                        _networkIdManager.GetNewNetId(), turret.Team, turret.Position,
                        25000.0f, 800.0f, -2, 
                        null, turret, turret.CharData.PathfindingCollisionRadius,
                        130.0f, 1.0f, 0,
                        true, true
                    );

                    // To suppress game HP-related errors for enemy turrets out of vision
                    _game.PacketNotifier.NotifyEnterLocalVisibilityClient(turret, userId);

                    foreach (var item in turret.Inventory)
                    {
                        if (item == null)
                        {
                            continue;
                        }

                        _game.PacketNotifier.NotifyBuyItem((int)turret.NetId, turret, item as IItem);
                    }
                }
                else if (kv.Value is ILevelProp levelProp)
                {
                     _game.PacketNotifier.NotifyLevelPropSpawn(userId, levelProp);
                }
                else if (kv.Value is IInhibitor || kv.Value is INexus)
                {
                    var inhibtor = (IAttackableUnit)kv.Value;
                     _game.PacketNotifier.NotifyStaticObjectSpawn(userId, inhibtor.NetId);
                    _game.PacketNotifier.NotifyEnterLocalVisibilityClient(userId, inhibtor.NetId);
                }
                else if (kv.Value is ISpellMissile missile)
                {
                    if (missile.IsVisibleByTeam(peerInfo.Champion.Team))
                    {
                         _game.PacketNotifier.NotifyMissileReplication(missile);
                    }
                }
                else
                {
                    _logger.Warn("Object of type: " + kv.Value.GetType() + " not handled in HandleSpawn.");
                }
            }

            // TODO shop map specific?
            // Level props are just models, we need button-object minions to allow the client to interact with it
            // TODO: Generate shop NetId to avoid hard-coding
            if (peerInfo != null && peerInfo.Team == TeamId.TEAM_BLUE)
            {
                // Shop (blue team)
                _game.PacketNotifier.NotifyStaticObjectSpawn(userId, 0xff10c6db);
                _game.PacketNotifier.NotifyEnterLocalVisibilityClient(userId, 0xff10c6db);
            }
            else if (peerInfo != null && peerInfo.Team == TeamId.TEAM_PURPLE)
            {
                // Shop (purple team)
                _game.PacketNotifier.NotifyStaticObjectSpawn(userId, 0xffa6170e);
                _game.PacketNotifier.NotifyEnterLocalVisibilityClient(userId, 0xffa6170e);
            }

             _game.PacketNotifier.NotifySpawnEnd(userId);
            return true;
        }
    }
}
