using ENet;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.Buildings.AnimatedBuildings;
using LeagueSandbox.GameServer.Logic.GameObjects.Missiles;
using LeagueSandbox.GameServer.Logic.Logging;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleSpawn : PacketHandlerBase
    {
        private IPacketNotifier _packetNotifier;
        private readonly ILogger _logger;
        private readonly Game _game;
        private readonly ItemManager _itemManager;
        private readonly PlayerManager _playerManager;
        private readonly NetworkIdManager _networkIdManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_CHAR_LOADED;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleSpawn(Game game)
        {
            _packetNotifier = game.PacketNotifier;
            _logger = LoggerProvider.GetLogger();
            _game = game;
            _itemManager = game.ItemManager;
            _playerManager = game.PlayerManager;
            _networkIdManager = game.NetworkIdManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            _packetNotifier.NotifySpawnStart(peer);
            _logger.Info("Spawning map");

            var playerId = 0;
            foreach (var p in _playerManager.GetPlayers())
            {
                _packetNotifier.NotifyHeroSpawn(peer, p.Item2, playerId++);
                _packetNotifier.NotifyAvatarInfo(peer, p.Item2);
            }

            var peerInfo = _playerManager.GetPeerInfo(peer);
            var bluePill = _itemManager.GetItemType(_game.Map.MapGameScript.BluePillId);
            var itemInstance = peerInfo.Champion.GetInventory().SetExtraItem(7, bluePill);

            _packetNotifier.NotifyBuyItem(peer, peerInfo.Champion, itemInstance);

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
            _packetNotifier.NotifySkillUp(peer, peerInfo.Champion.NetId, 7, 1, (byte)peerInfo.Champion.GetSkillPoints());
            _packetNotifier.NotifySkillUp(peer, peerInfo.Champion.NetId, 14, 1, (byte)peerInfo.Champion.GetSkillPoints());

            peerInfo.Champion.Stats.SetSpellEnabled(7, true);
            peerInfo.Champion.Stats.SetSpellEnabled(14, true);
            peerInfo.Champion.Stats.SetSummonerSpellEnabled(0, true);
            peerInfo.Champion.Stats.SetSummonerSpellEnabled(1, true);

            var objects = _game.ObjectManager.GetObjects();
            foreach (var kv in objects)
            {
                if (kv.Value is LaneTurret turret)
                {
                    _packetNotifier.NotifyTurretSpawn(peer, turret);

                    // Fog Of War
                    _packetNotifier.NotifyFogUpdate2(turret);

                    // To suppress game HP-related errors for enemy turrets out of vision
                    _packetNotifier.NotifySetHealth(peer, turret);

                    foreach (var item in turret.Inventory)
                    {
                        if (item == null) continue;
                        _packetNotifier.NotifyItemBought(turret, item as Item);
                    }
                }
                else if (kv.Value is LevelProp levelProp)
                {
                    _packetNotifier.NotifyLevelPropSpawn(peer, levelProp);
                }
                else if (kv.Value is Champion champion)
                {
                    if (champion.IsVisibleByTeam(peerInfo.Champion.Team))
                    {
                        _packetNotifier.NotifyEnterVision(peer, champion);
                    }
                }
                else if (kv.Value is Inhibitor || kv.Value is Nexus)
                {
                    var inhibtor = (AttackableUnit)kv.Value;
                    _packetNotifier.NotifyStaticObjectSpawn(peer, inhibtor.NetId);
                    _packetNotifier.NotifySetHealth(peer, inhibtor.NetId);
                }
                else if (kv.Value is Projectile projectile)
                {
                    if (projectile.IsVisibleByTeam(peerInfo.Champion.Team))
                    {
                        _packetNotifier.NotifyProjectileSpawn(peer, projectile);
                    }
                }
                else
                {
                    _logger.Warning("Object of type: " + kv.Value.GetType() + " not handled in HandleSpawn.");
                }
            }

            // TODO shop map specific?
            // Level props are just models, we need button-object minions to allow the client to interact with it
            if (peerInfo != null && peerInfo.Team == TeamId.TEAM_BLUE)
            {
                // Shop (blue team)
                _packetNotifier.NotifyStaticObjectSpawn(peer, 0xff10c6db);
                _packetNotifier.NotifySetHealth(peer, 0xff10c6db);
            }
            else if (peerInfo != null && peerInfo.Team == TeamId.TEAM_PURPLE)
            {
                // Shop (purple team)
                _packetNotifier.NotifyStaticObjectSpawn(peer, 0xffa6170e);
                _packetNotifier.NotifySetHealth(peer, 0xffa6170e);
            }

            _packetNotifier.NotifySpawnEnd(peer);
            return true;
        }
    }
}
