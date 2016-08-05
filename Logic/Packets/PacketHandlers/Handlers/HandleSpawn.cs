﻿using System;
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
        public bool HandlePacket(Peer peer, byte[] data, Game game)
        {
            var start = new StatePacket2(PacketCmdS2C.PKT_S2C_StartSpawn);
            game.PacketHandlerManager.sendPacket(peer, start, Channel.CHL_S2C);
            Logger.LogCoreInfo("Spawning map");

            int playerId = 0;
            foreach (var p in game.GetPlayers())
            {
                var spawn = new HeroSpawn(p.Item2, playerId++);
                game.PacketHandlerManager.sendPacket(peer, spawn, Channel.CHL_S2C);

                var info = new PlayerInfo(p.Item2);
                game.PacketHandlerManager.sendPacket(peer, info, Channel.CHL_S2C);
            }

            var peerInfo = game.GetPeerInfo(peer);
            var bluePill = game.ItemManager.GetItemType(game.GetMap().GetBluePillId());
            var itemInstance = peerInfo.GetChampion().Inventory.SetExtraItem(7, bluePill);
            var buyItem = new BuyItemAns(peerInfo.GetChampion(), itemInstance);
            game.PacketHandlerManager.sendPacket(peer, buyItem, Channel.CHL_S2C);

            // Not sure why both 7 and 14 skill slot, but it does not seem to work without it
            var skillUp = new SkillUpPacket(peerInfo.GetChampion().getNetId(), 7, 1, (byte)peerInfo.GetChampion().getSkillPoints());
            game.PacketHandlerManager.sendPacket(peer, skillUp, Channel.CHL_GAMEPLAY);
            skillUp = new SkillUpPacket(peerInfo.GetChampion().getNetId(), 14, 1, (byte)peerInfo.GetChampion().getSkillPoints());
            game.PacketHandlerManager.sendPacket(peer, skillUp, Channel.CHL_GAMEPLAY);

            peerInfo.GetChampion().GetStats().setSpellEnabled(7, true);
            peerInfo.GetChampion().GetStats().setSpellEnabled(14, true);
            peerInfo.GetChampion().GetStats().setSummonerSpellEnabled(0, true);
            peerInfo.GetChampion().GetStats().setSummonerSpellEnabled(1, true);

            var objects = game.GetMap().GetObjects();
            foreach (var kv in objects)
            {
                if (kv.Value is Turret)
                {
                    var t = kv.Value as Turret;
                    var turretSpawn = new TurretSpawn(t);
                    game.PacketHandlerManager.sendPacket(peer, turretSpawn, Channel.CHL_S2C);

                    // To suppress game HP-related errors for enemy turrets out of vision
                    var sh = new SetHealth(t);
                    game.PacketHandlerManager.sendPacket(peer, sh, Channel.CHL_S2C);
                    continue;
                }
                else if (kv.Value is LevelProp)
                {
                    var lp = kv.Value as LevelProp;

                    var lpsPacket = new LevelPropSpawn(lp);
                    game.PacketHandlerManager.sendPacket(peer, lpsPacket, Channel.CHL_S2C);
                }
                else if (kv.Value is Inhibitor || kv.Value is Nexus)
                {
                    var inhib = kv.Value as Unit;

                    var ms = new MinionSpawn2(inhib.getNetId());
                    game.PacketHandlerManager.sendPacket(peer, ms, Channel.CHL_S2C);
                    var sh = new SetHealth(inhib.getNetId());
                    game.PacketHandlerManager.sendPacket(peer, sh, Channel.CHL_S2C);
                }
            }

            // TODO shop map specific?
            // Level props are just models, we need button-object minions to allow the client to interact with it
            if (peerInfo != null && peerInfo.GetTeam() == TeamId.TEAM_BLUE)
            {
                // Shop (blue team)
                var ms1 = new MinionSpawn2(0xff10c6db);
                game.PacketHandlerManager.sendPacket(peer, ms1, Channel.CHL_S2C);
                var sh1 = new SetHealth(0xff10c6db);
                game.PacketHandlerManager.sendPacket(peer, sh1, Channel.CHL_S2C);
            }
            else if (peerInfo != null && peerInfo.GetTeam() == TeamId.TEAM_PURPLE)
            {
                // Shop (purple team)
                var ms1 = new MinionSpawn2(0xffa6170e);
                game.PacketHandlerManager.sendPacket(peer, ms1, Channel.CHL_S2C);
                var sh1 = new SetHealth(0xffa6170e);
                game.PacketHandlerManager.sendPacket(peer, sh1, Channel.CHL_S2C);
            }

            var end = new StatePacket(PacketCmdS2C.PKT_S2C_EndSpawn);
            return game.PacketHandlerManager.sendPacket(peer, end, Channel.CHL_S2C);
        }
    }
}