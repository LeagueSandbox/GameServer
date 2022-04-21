using GameServerCore.Packets.PacketDefinitions.Requests;
using GameServerCore;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Handlers;
using LeagueSandbox.GameServer.Inventory;
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
            var players = _playerManager.GetPlayers(true);

            if (_firstSpawn)
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
                var champ = peerInfo.Champion;

                _game.PacketNotifier.NotifyS2C_CreateHero(peerInfo, userId);
                _game.PacketNotifier.NotifyAvatarInfo(peerInfo, userId);

                // Buy blue pill
                var itemInstance = peerInfo.Champion.Inventory.GetItem(7);
                _game.PacketNotifier.NotifyBuyItem(userId, peerInfo.Champion, itemInstance);

                champ.SetSpawnedForPlayer(userId);

                if (_game.IsRunning)
                {
                    bool ownChamp = peerInfo.PlayerId == userId;
                    if (ownChamp || champ.IsVisibleForPlayer(userId))
                    {
                        _game.PacketNotifier.NotifyVisibilityChange(champ, userInfo.Team, true, userId);
                    }
                    if (ownChamp)
                    {
                        // Set spell levels
                        foreach (var spell in champ.Spells)
                        {
                            var castInfo = spell.Value.CastInfo;
                            if (castInfo.SpellLevel > 0)
                            {
                                // NotifyNPC_UpgradeSpellAns has no effect here 
                                _game.PacketNotifier.NotifyS2C_SetSpellLevel(userId, champ.NetId, castInfo.SpellSlot, castInfo.SpellLevel);

                                float currentCD = spell.Value.CurrentCooldown;
                                float totalCD = spell.Value.GetCooldown();
                                if (currentCD > 0)
                                {
                                    _game.PacketNotifier.NotifyCHAR_SetCooldown(champ, castInfo.SpellSlot, currentCD, totalCD, userId);
                                }
                            }
                        }
                    }
                }
            }

            var objects = _game.ObjectManager.GetObjects();
            foreach (var obj in objects.Values)
            {
                if (!(obj is IChampion))
                {
                    if (_game.IsRunning)
                    {
                        if (obj.IsSpawnedForPlayer(userId))
                        {
                            bool isVisibleForPlayer = obj.IsVisibleForPlayer(userId);
                            _game.PacketNotifier.NotifySpawn(obj, userInfo.Team, userId, _game.GameTime, isVisibleForPlayer);
                        }
                    }
                    else
                    {
                        (_game.ObjectManager as ObjectManager).UpdateVisionSpawnAndSync(obj, userInfo, forceSpawn: true);
                    }
                }
            }

            _game.PacketNotifier.NotifySpawnEnd(userId);
            return true;
        }
    }
}