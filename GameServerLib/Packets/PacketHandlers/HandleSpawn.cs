using GameServerCore;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
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
        private readonly INetworkIdManager _networkIdManager;

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
             _game.PacketNotifier.NotifySpawnStart(userId);
            _logger.Debug("Spawning map");

            var playerId = 0;
            foreach (var p in _playerManager.GetPlayers())
            {
                 _game.PacketNotifier.NotifyHeroSpawn(userId, p.Item2, playerId++);
                 _game.PacketNotifier.NotifyAvatarInfo(userId, p.Item2);
            }

            var peerInfo = _playerManager.GetPeerInfo(userId);
            var bluePill = _itemManager.GetItemType(_game.Map.MapProperties.BluePillId);
            var itemInstance = peerInfo.Champion.Inventory.SetExtraItem(7, bluePill);

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

            // Not sure why both 7 and 14 skill slot, but it does not seem to work without it
             _game.PacketNotifier.NotifySkillUp(userId, peerInfo.Champion.NetId, 7, 1, peerInfo.Champion.SkillPoints);
             _game.PacketNotifier.NotifySkillUp(userId, peerInfo.Champion.NetId, 14, 1, peerInfo.Champion.SkillPoints);

            peerInfo.Champion.Stats.SetSpellEnabled(7, true);
            peerInfo.Champion.Stats.SetSpellEnabled(14, true);
            peerInfo.Champion.Stats.SetSummonerSpellEnabled(0, true);
            peerInfo.Champion.Stats.SetSummonerSpellEnabled(1, true);

            var objects = _game.ObjectManager.GetObjects();
            foreach (var kv in objects)
            {
                if (kv.Value is ILaneTurret turret)
                {
                     _game.PacketNotifier.NotifyTurretSpawn(userId, turret);

                    // Fog Of War
                     _game.PacketNotifier.NotifyFogUpdate2(turret, _networkIdManager.GetNewNetId());

                    // To suppress game HP-related errors for enemy turrets out of vision
                     _game.PacketNotifier.NotifySetHealth(userId, turret);

                    foreach (var item in turret.Inventory)
                    {
                        if (item == null) continue;
                         _game.PacketNotifier.NotifyItemBought(turret, item as IItem);
                    }
                }
                else if (kv.Value is ILevelProp levelProp)
                {
                     _game.PacketNotifier.NotifyLevelPropSpawn(userId, levelProp);
                }
                else if (kv.Value is IChampion champion)
                {
                    if (champion.IsVisibleByTeam(peerInfo.Champion.Team))
                    {
                         _game.PacketNotifier.NotifyEnterVision(userId, champion);
                    }
                }
                else if (kv.Value is IInhibitor || kv.Value is INexus)
                {
                    var inhibtor = (IAttackableUnit)kv.Value;
                     _game.PacketNotifier.NotifyStaticObjectSpawn(userId, inhibtor.NetId);
                     _game.PacketNotifier.NotifySetHealth(userId, inhibtor.NetId);
                }
                else if (kv.Value is IProjectile projectile)
                {
                    if (projectile.IsVisibleByTeam(peerInfo.Champion.Team))
                    {
                         _game.PacketNotifier.NotifyProjectileSpawn(userId, projectile);
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
                 _game.PacketNotifier.NotifyStaticObjectSpawn(userId, 0xff10c6db);
                 _game.PacketNotifier.NotifySetHealth(userId, 0xff10c6db);
            }
            else if (peerInfo != null && peerInfo.Team == TeamId.TEAM_PURPLE)
            {
                // Shop (purple team)
                 _game.PacketNotifier.NotifyStaticObjectSpawn(userId, 0xffa6170e);
                 _game.PacketNotifier.NotifySetHealth(userId, 0xffa6170e);
            }

             _game.PacketNotifier.NotifySpawnEnd(userId);
            return true;
        }
    }
}
