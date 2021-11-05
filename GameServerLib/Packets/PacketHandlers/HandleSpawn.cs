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
            var bluePill = _itemManager.GetItemType(_game.Map.MapScript.BluePillId);
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
                        _game.PacketNotifier.NotifyEnterVisibilityClient(champion, userId, false, false, true);
                    }
                }
                else if (kv.Value is ILaneTurret turret)
                {
                    _game.PacketNotifier.NotifyS2C_CreateTurret(turret, userId);
                }
                else if (kv.Value is ILevelProp levelProp)
                {
                    _game.PacketNotifier.NotifySpawnLevelPropS2C(levelProp, userId);
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