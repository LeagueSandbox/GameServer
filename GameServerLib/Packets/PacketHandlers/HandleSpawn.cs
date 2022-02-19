using GameServerCore;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Enums;
using GameServerCore.Packets.Handlers;
using GameServerCore.Packets.PacketDefinitions.Requests;
using LeagueSandbox.GameServer.GameObjects;
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

        private bool _firstSpawn = true;
        public override bool HandlePacket(int userId, SpawnRequest req)
        {
            var packetNotifier = _game.PacketNotifier as PacketDefinitions420.PacketNotifier;

            var players = _playerManager.GetPlayers(true);

            if(_firstSpawn)
            {
                _firstSpawn = false;

                var bluePill = _itemManager.GetItemType(_game.Map.MapScript.MapScriptMetadata.RecallSpellItemId);

                foreach (var kv in players)
                {
                    var peerInfo = kv.Item2;

                    var itemInstance = peerInfo.Champion.Inventory.SetExtraItem(7, bluePill);

                    // Runes
                    byte runeItemSlot = 14;
                    foreach (var rune in peerInfo.Champion.RuneList.Runes)
                    {
                        var runeItem = _itemManager.GetItemType(rune.Value);
                        var newRune = peerInfo.Champion.Inventory.SetExtraItem(runeItemSlot, runeItem);
                        peerInfo.Champion.Stats.AddModifier(runeItem);
                        runeItemSlot++;
                    }

                    peerInfo.Champion.Stats.SetSummonerSpellEnabled(0, true);
                    peerInfo.Champion.Stats.SetSummonerSpellEnabled(1, true);

                    _game.ObjectManager.AddObject(peerInfo.Champion);
                }
            }

            _logger.Debug("Spawning map");
            _game.PacketNotifier.NotifyS2C_StartSpawn(userId);

            var userInfo = _playerManager.GetPeerInfo(userId);

            foreach (var kv in players)
            {
                var peerInfo = kv.Item2;
                var champ = peerInfo.Champion as GameObject;

                _game.PacketNotifier.NotifyS2C_CreateHero(peerInfo, userId);
                _game.PacketNotifier.NotifyAvatarInfo(peerInfo, userId);

                // Buy blue pill
                var itemInstance = peerInfo.Champion.Inventory.GetItem(7);
                _game.PacketNotifier.NotifyBuyItem(userId, peerInfo.Champion, itemInstance);
                
                champ.SetSpawnedForPlayer(userId);

                if(_game.IsRunning && champ.IsVisibleForPlayer(userId))
                {
                    packetNotifier.NotifyVisibilityChange(champ, userInfo.Team, userId, true);
                }
            }

            var objects = _game.ObjectManager.GetObjects();
            foreach (var _obj in objects.Values)
            {
                var obj = _obj as GameObject;
                if(!(obj is IChampion))
                {
                    if(_game.IsRunning)
                    {
                        if(obj.IsSpawnedForPlayer(userId))
                        {
                            packetNotifier.NotifySpawn(obj, userId, obj.IsVisibleForPlayer(userId), _game.GameTime);
                        }
                    }
                    else
                    {
                        (_game.ObjectManager as ObjectManager)
                        .UpdateVisibilityAndSpawnIfNeeded(obj, userInfo, forceSpawn: true);
                    }
                }
            }

            //TODO: shop map specific?
            // Level props are just models, we need button-object minions to allow the client to interact with it
            _game.PacketNotifier.NotifySpawn(_game.Map.ShopList[userInfo.Team], userId, false);

            _game.PacketNotifier.NotifySpawnEnd(userId);
            return true;
        }
    }
}