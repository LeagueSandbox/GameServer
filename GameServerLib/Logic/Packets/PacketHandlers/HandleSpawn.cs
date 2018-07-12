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

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_CHAR_LOADED;
        public override Channel PacketChannel => Channel.CHL_C2_S;

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var start = new StatePacket2(PacketCmd.PKT_S2C_START_SPAWN);
            Game.PacketHandlerManager.SendPacket(peer, start, Channel.CHL_S2_C);
            Logger.LogCoreInfo("Spawning map");

            var playerId = 0;
            foreach (var p in PlayerManager.GetPlayers())
            {
                var spawn = new HeroSpawn(p.Item2, playerId++);
                Game.PacketHandlerManager.SendPacket(peer, spawn, Channel.CHL_S2_C);

                var info = new AvatarInfo(p.Item2);
                Game.PacketHandlerManager.SendPacket(peer, info, Channel.CHL_S2_C);
            }

            var peerInfo = PlayerManager.GetPeerInfo(peer);
            var bluePill = ItemManager.GetItemType(Game.Map.MapGameScript.BluePillId);
            var itemInstance = peerInfo.Champion.GetInventory().SetExtraItem(7, bluePill);
            var buyItem = new BuyItemResponse(peerInfo.Champion, itemInstance);
            Game.PacketHandlerManager.SendPacket(peer, buyItem, Channel.CHL_S2_C);

            // Runes
            byte runeItemSlot = 14;
            foreach (var rune in peerInfo.Champion.RuneList.Runes)
            {
                var runeItem = ItemManager.GetItemType(rune.Value);
                var newRune = peerInfo.Champion.GetInventory().SetExtraItem(runeItemSlot, runeItem);
                PlayerManager.GetPeerInfo(peer).Champion.Stats.AddModifier(runeItem);
                runeItemSlot++;
            }

            // Not sure why both 7 and 14 skill slot, but it does not seem to work without it
            var skillUp = new SkillUpResponse(peerInfo.Champion.NetId, 7, 1, (byte)peerInfo.Champion.GetSkillPoints());
            Game.PacketHandlerManager.SendPacket(peer, skillUp, Channel.CHLGamePLAY);
            skillUp = new SkillUpResponse(peerInfo.Champion.NetId, 14, 1, (byte)peerInfo.Champion.GetSkillPoints());
            Game.PacketHandlerManager.SendPacket(peer, skillUp, Channel.CHLGamePLAY);

            peerInfo.Champion.Stats.SetSpellEnabled(7, true);
            peerInfo.Champion.Stats.SetSpellEnabled(14, true);
            peerInfo.Champion.Stats.SetSummonerSpellEnabled(0, true);
            peerInfo.Champion.Stats.SetSummonerSpellEnabled(1, true);

            var objects = Game.ObjectManager.GetObjects();
            foreach (var kv in objects)
            {
                if (kv.Value is LaneTurret turret)
                {
                    var turretSpawn = new TurretSpawn(turret);
                    Game.PacketHandlerManager.SendPacket(peer, turretSpawn, Channel.CHL_S2_C);

                    // Fog Of War
                    var fogOfWarPacket = new FogUpdate2(turret);
                    Game.PacketHandlerManager.BroadcastPacketTeam(turret.Team, fogOfWarPacket, Channel.CHL_S2_C);

                    // To suppress game HP-related errors for enemy turrets out of vision
                    var setHealthPacket = new SetHealth(turret);
                    Game.PacketHandlerManager.SendPacket(peer, setHealthPacket, Channel.CHL_S2_C);

                    foreach (var item in turret.Inventory)
                    {
                        if (item == null) continue;
                        Game.PacketNotifier.NotifyItemBought(turret, item as Item);
                    }
                }
                else if (kv.Value is LevelProp levelProp)
                {
                    var levelPropSpawnPacket = new LevelPropSpawn(levelProp);
                    Game.PacketHandlerManager.SendPacket(peer, levelPropSpawnPacket, Channel.CHL_S2_C);
                }
                else if (kv.Value is Champion champion)
                {
                    if (champion.IsVisibleByTeam(peerInfo.Champion.Team))
                    {
                        var enterVisionPacket = new EnterVisionAgain(champion);
                        Game.PacketHandlerManager.SendPacket(peer, enterVisionPacket, Channel.CHL_S2_C);
                    }
                }
                else if (kv.Value is Inhibitor || kv.Value is Nexus)
                {
                    var inhibtor = (AttackableUnit) kv.Value;

                    var minionSpawnPacket = new MinionSpawn2(inhibtor.NetId);
                    Game.PacketHandlerManager.SendPacket(peer, minionSpawnPacket, Channel.CHL_S2_C);
                    var setHealthPacket = new SetHealth(inhibtor.NetId);
                    Game.PacketHandlerManager.SendPacket(peer, setHealthPacket, Channel.CHL_S2_C);
                }
                else if (kv.Value is Projectile projectile)
                {
                    if (projectile.IsVisibleByTeam(peerInfo.Champion.Team))
                    {
                        var spawnProjectilePacket = new SpawnProjectile(projectile);
                        Game.PacketHandlerManager.SendPacket(peer, spawnProjectilePacket, Channel.CHL_S2_C);
                    }
                }
                else
                {
                    Logger.LogCoreWarning("Object of type: " + kv.Value.GetType() + " not handled in HandleSpawn.");
                }
            }

            // TODO shop map specific?
            // Level props are just models, we need button-object minions to allow the client to interact with it
            if (peerInfo.Team == TeamId.TEAM_BLUE)
            {
                // Shop (blue team)
                var minionSpawnPacket = new MinionSpawn2(0xff10c6db);
                Game.PacketHandlerManager.SendPacket(peer, minionSpawnPacket, Channel.CHL_S2_C);
                var setHealthPacket = new SetHealth(0xff10c6db);
                Game.PacketHandlerManager.SendPacket(peer, setHealthPacket, Channel.CHL_S2_C);
            }
            else if (peerInfo.Team == TeamId.TEAM_PURPLE)
            {
                // Shop (purple team)
                var minionSpawnPacket = new MinionSpawn2(0xffa6170e);
                Game.PacketHandlerManager.SendPacket(peer, minionSpawnPacket, Channel.CHL_S2_C);
                var setHealthPacket = new SetHealth(0xffa6170e);
                Game.PacketHandlerManager.SendPacket(peer, setHealthPacket, Channel.CHL_S2_C);
            }

            var endSpawnPacket = new StatePacket(PacketCmd.PKT_S2C_END_SPAWN);
            return Game.PacketHandlerManager.SendPacket(peer, endSpawnPacket, Channel.CHL_S2_C);
        }
    }
}
