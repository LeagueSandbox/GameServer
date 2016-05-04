using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Items;
using LeagueSandbox.GameServer.Logic.Content;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleSpawn : IPacketHandler
    {
        public  bool HandlePacket(Peer peer, byte[] data, Game game)
        {
            var start = new StatePacket2(PacketCmdS2C.PKT_S2C_StartSpawn);
            PacketHandlerManager.getInstace().sendPacket(peer, start, Channel.CHL_S2C);
            Logger.LogCoreInfo("Spawning map");

            int playerId = 0;
            foreach (var p in game.getPlayers())
            {
                var spawn = new HeroSpawn(p.Item2, playerId++);
                PacketHandlerManager.getInstace().sendPacket(peer, spawn, Channel.CHL_S2C);

                var info = new PlayerInfo(p.Item2);
                PacketHandlerManager.getInstace().sendPacket(peer, info, Channel.CHL_S2C);
            }

            var peerInfo = game.getPeerInfo(peer);
            var bluePill = game.ItemManager.GetItemType(game.getMap().getBluePillId());
            var itemInstance = peerInfo.getChampion().Inventory.SetExtraItem(7, bluePill);
            var buyItem = new BuyItemAns(peerInfo.getChampion(), itemInstance);
            PacketHandlerManager.getInstace().sendPacket(peer, buyItem, Channel.CHL_S2C);

            // Not sure why both 7 and 14 skill slot, but it does not seem to work without it
            var skillUp = new SkillUpPacket(peerInfo.getChampion().getNetId(), 7, 1, (byte)peerInfo.getChampion().getSkillPoints());
            PacketHandlerManager.getInstace().sendPacket(peer, skillUp, Channel.CHL_GAMEPLAY);
            skillUp = new SkillUpPacket(peerInfo.getChampion().getNetId(), 14, 1, (byte)peerInfo.getChampion().getSkillPoints());
            PacketHandlerManager.getInstace().sendPacket(peer, skillUp, Channel.CHL_GAMEPLAY);

            peerInfo.getChampion().getStats().setSpellEnabled(7, true);
            peerInfo.getChampion().getStats().setSpellEnabled(14, true);
            peerInfo.getChampion().getStats().setSummonerSpellEnabled(0, true);
            peerInfo.getChampion().getStats().setSummonerSpellEnabled(1, true);

            var objects = game.getMap().GetObjects();
            foreach (var kv in objects)
            {
                if (kv.Value is Turret)
                {
                    var Turret = kv.Value as Turret;
                    var TurretSpawnPacket = new TurretSpawn(Turret);
                    PacketHandlerManager.getInstace().sendPacket(peer, TurretSpawnPacket, Channel.CHL_S2C);

                    // To suppress game HP-related errors for enemy turrets out of vision
                    var SetHealthPacket = new SetHealth(Turret);
                    PacketHandlerManager.getInstace().sendPacket(peer, SetHealthPacket, Channel.CHL_S2C);

                    /*var Unit = kv.Value as Unit;
                    if (Unit != null)
                    {
                        PacketNotifier.notifyUpdatedStats(Unit, false);
                    }*/

                    continue;
                }
                else if (kv.Value is LevelProp)
                {
                    var LevelProp = kv.Value as LevelProp;

                    var LevelPropSpawnPacket = new LevelPropSpawn(LevelProp);
                    PacketHandlerManager.getInstace().sendPacket(peer, LevelPropSpawnPacket, Channel.CHL_S2C);
                    continue;
                }
                else if (kv.Value is Inhibitor || kv.Value is Nexus)
                {
                    var InhibitorOrNexus = kv.Value as Unit;

                    var InhibitorOrNexusSpawnPacket = new MinionSpawn2(InhibitorOrNexus.getNetId());
                    PacketHandlerManager.getInstace().sendPacket(peer, InhibitorOrNexusSpawnPacket, Channel.CHL_S2C);
                    var SetHealthPacket = new SetHealth(InhibitorOrNexus.getNetId());
                    PacketHandlerManager.getInstace().sendPacket(peer, SetHealthPacket, Channel.CHL_S2C);

                    /*var Unit = kv.Value as Unit;
                    if (Unit != null)
                    {
                        PacketNotifier.notifyUpdatedStats(Unit, false);
                    }*/

                    continue;
                }
                else if (kv.Value is Minion)
                {
                    var Minion = kv.Value as Minion;
                    if (Minion.isVisibleByTeam(game.getPeerInfo(peer).getTeam()))
                    {
                        var MinionSpawnPacket = new MinionSpawn(Minion);
                        PacketHandlerManager.getInstace().sendPacket(peer, MinionSpawnPacket, Channel.CHL_S2C);
                        var SetHealthPacket = new SetHealth(Minion);
                        PacketHandlerManager.getInstace().sendPacket(peer, SetHealthPacket, Channel.CHL_S2C);

                        /*var Unit = kv.Value as Unit;
                        if (Unit != null)
                        {
                            PacketNotifier.notifyUpdatedStats(Unit, false);
                        }*/
                    }
                    continue;
                }
                else if (kv.Value is Champion)
                {
                    var Champion = kv.Value as Champion;
                    if (Champion.isVisibleByTeam(game.getPeerInfo(peer).getTeam()))
                    {
                        var ChampionRespawnPacket = new ChampionRespawn(Champion);
                        PacketHandlerManager.getInstace().sendPacket(peer, ChampionRespawnPacket, Channel.CHL_S2C);
                        var SetHealthPacket = new SetHealth(Champion);
                        PacketHandlerManager.getInstace().sendPacket(peer, SetHealthPacket, Channel.CHL_S2C);

                        /*var Unit = kv.Value as Unit;
                        if (Unit != null)
                        {
                            PacketNotifier.notifyUpdatedStats(Unit, false);
                        }*/
                    }
                    continue;
                }
                else if (kv.Value is Projectile)
                {
                    var Projectile = kv.Value as Projectile;
                    if (Projectile.isVisibleByTeam(game.getPeerInfo(peer).getTeam()))
                    {
                        var SpawnProjectilePacket = new SpawnProjectile(Projectile);
                        PacketHandlerManager.getInstace().sendPacket(peer, SpawnProjectilePacket, Channel.CHL_S2C);
                    }
                    continue;
                }
                else
                {
                    Logger.LogCoreWarning("Object of type: " + kv.Value.GetType() + " not handled in HandleSpawn.");
                }
            }

            // TODO shop map specific?
            // Level props are just models, we need button-object minions to allow the client to interact with it
            if (peerInfo != null && peerInfo.getTeam() == TeamId.TEAM_BLUE)
            {
                // Shop (blue team)
                var ms1 = new MinionSpawn2(0xff10c6db);
                PacketHandlerManager.getInstace().sendPacket(peer, ms1, Channel.CHL_S2C);
                var sh1 = new SetHealth(0xff10c6db);
                PacketHandlerManager.getInstace().sendPacket(peer, sh1, Channel.CHL_S2C);
            }
            else if (peerInfo != null && peerInfo.getTeam() == TeamId.TEAM_PURPLE)
            {
                // Shop (purple team)
                var ms1 = new MinionSpawn2(0xffa6170e);
                PacketHandlerManager.getInstace().sendPacket(peer, ms1, Channel.CHL_S2C);
                var sh1 = new SetHealth(0xffa6170e);
                PacketHandlerManager.getInstace().sendPacket(peer, sh1, Channel.CHL_S2C);
            }

            var end = new StatePacket(PacketCmdS2C.PKT_S2C_EndSpawn);
            return PacketHandlerManager.getInstace().sendPacket(peer, end, Channel.CHL_S2C);
        }
    }
}