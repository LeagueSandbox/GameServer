using ENet;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleSpawn : IPacketHandler
    {
        private Logger _logger = Program.ResolveDependency<Logger>();
        private Game _game = Program.ResolveDependency<Game>();
        private ItemManager _itemManager = Program.ResolveDependency<ItemManager>();
        private PlayerManager _playerManager = Program.ResolveDependency<PlayerManager>();

        public bool HandlePacket(Peer peer, byte[] data)
        {
            var start = new StatePacket2(PacketCmdS2C.PKT_S2C_StartSpawn);
            _game.PacketHandlerManager.sendPacket(peer, start, Channel.CHL_S2C);
            _logger.LogCoreInfo("Spawning map");

            int playerId = 0;
            foreach (var p in _playerManager.GetPlayers())
            {
                var spawn = new HeroSpawn(p.Item2, playerId++);
                _game.PacketHandlerManager.sendPacket(peer, spawn, Channel.CHL_S2C);

                var info = new PlayerInfo(p.Item2);
                _game.PacketHandlerManager.sendPacket(peer, info, Channel.CHL_S2C);
            }

            var peerInfo = _playerManager.GetPeerInfo(peer);
            var bluePill = _itemManager.GetItemType(_game.Map.GetBluePillId());
            var itemInstance = peerInfo.Champion.getInventory().SetExtraItem(7, bluePill);
            var buyItem = new BuyItemAns(peerInfo.Champion, itemInstance);
            _game.PacketHandlerManager.sendPacket(peer, buyItem, Channel.CHL_S2C);

            // Runes
            byte runeItemSlot = 14;
            foreach (var rune in peerInfo.Champion.RuneList._runes)
            {
                var runeItem = _itemManager.GetItemType(rune.Value);
                var newRune = peerInfo.Champion.getInventory().SetExtraItem(runeItemSlot, runeItem);
                _playerManager.GetPeerInfo(peer).Champion.GetStats().AddBuff(runeItem);
                runeItemSlot++;
            }

            // Not sure why both 7 and 14 skill slot, but it does not seem to work without it
            var skillUp = new SkillUpPacket(peerInfo.Champion.NetId, 7, 1, (byte)peerInfo.Champion.getSkillPoints());
            _game.PacketHandlerManager.sendPacket(peer, skillUp, Channel.CHL_GAMEPLAY);
            skillUp = new SkillUpPacket(peerInfo.Champion.NetId, 14, 1, (byte)peerInfo.Champion.getSkillPoints());
            _game.PacketHandlerManager.sendPacket(peer, skillUp, Channel.CHL_GAMEPLAY);

            peerInfo.Champion.GetStats().setSpellEnabled(7, true);
            peerInfo.Champion.GetStats().setSpellEnabled(14, true);
            peerInfo.Champion.GetStats().setSummonerSpellEnabled(0, true);
            peerInfo.Champion.GetStats().setSummonerSpellEnabled(1, true);

            var objects = _game.Map.GetObjects();
            foreach (var kv in objects)
            {
                if (kv.Value is LaneTurret)
                {
                    var t = kv.Value as LaneTurret;
                    var turretSpawn = new TurretSpawn(t);
                    _game.PacketHandlerManager.sendPacket(peer, turretSpawn, Channel.CHL_S2C);

                    // Fog Of War
                    var fow = new FogUpdate2(t);
                    _game.PacketHandlerManager.broadcastPacketTeam(t.Team, fow, Channel.CHL_S2C);

                    // To suppress game HP-related errors for enemy turrets out of vision
                    var sh = new SetHealth(t);
                    _game.PacketHandlerManager.sendPacket(peer, sh, Channel.CHL_S2C);
                    
                    foreach (var item in t.Inventory)
                    {
                        if (item == null) continue;
                        _game.PacketNotifier.notifyItemBought(t, item as Item);
                    }

                    continue;
                }
                else if (kv.Value is LevelProp)
                {
                    var lp = kv.Value as LevelProp;

                    var lpsPacket = new LevelPropSpawn(lp);
                    _game.PacketHandlerManager.sendPacket(peer, lpsPacket, Channel.CHL_S2C);
                }
                else if (kv.Value is Inhibitor || kv.Value is Nexus)
                {
                    var inhib = kv.Value as Unit;

                    var ms = new MinionSpawn2(inhib.NetId);
                    _game.PacketHandlerManager.sendPacket(peer, ms, Channel.CHL_S2C);
                    var sh = new SetHealth(inhib.NetId);
                    _game.PacketHandlerManager.sendPacket(peer, sh, Channel.CHL_S2C);
                }
            }

            // TODO shop map specific?
            // Level props are just models, we need button-object minions to allow the client to interact with it
            if (peerInfo != null && peerInfo.Team == TeamId.TEAM_BLUE)
            {
                // Shop (blue team)
                var ms1 = new MinionSpawn2(0xff10c6db);
                _game.PacketHandlerManager.sendPacket(peer, ms1, Channel.CHL_S2C);
                var sh1 = new SetHealth(0xff10c6db);
                _game.PacketHandlerManager.sendPacket(peer, sh1, Channel.CHL_S2C);
            }
            else if (peerInfo != null && peerInfo.Team == TeamId.TEAM_PURPLE)
            {
                // Shop (purple team)
                var ms1 = new MinionSpawn2(0xffa6170e);
                _game.PacketHandlerManager.sendPacket(peer, ms1, Channel.CHL_S2C);
                var sh1 = new SetHealth(0xffa6170e);
                _game.PacketHandlerManager.sendPacket(peer, sh1, Channel.CHL_S2C);
            }

            var end = new StatePacket(PacketCmdS2C.PKT_S2C_EndSpawn);
            return _game.PacketHandlerManager.sendPacket(peer, end, Channel.CHL_S2C);
        }
    }
}
