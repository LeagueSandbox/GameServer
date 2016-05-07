﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Enet;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleChatBoxMessage : IPacketHandler
    {
        public bool HandlePacket(Peer peer, byte[] data, Game game)
        {
            var message = new ChatMessage(data);
            var split = message.msg.Split(' ');
            if (split.Length > 1)
            {
                int x, y = 0;
                if (int.TryParse(split[0], out x))
                {
                    if (int.TryParse(split[1], out y))
                    {
                        var response = new AttentionPingAns(game.getPeerInfo(peer), new AttentionPing { x = x, y = y, targetNetId = 0, type = 0 });
                        PacketHandlerManager.getInstace().broadcastPacketTeam(game.getPeerInfo(peer).getTeam(), response, Channel.CHL_S2C);
                    }
                }
            }

            #region Commands
            //Lets do commands

            if (message.msg.StartsWith("."))
            {
                var cmd = new string[] { ".gold", ".speed", ".health", ".xp", ".ap", ".ad", ".mana", ".model", ".help", ".spawn", ".size", ".junglespawn", ".skillpoints", ".level", ".tp", ".coords", ".ch", ".reload" };
                var debugMsg = new StringBuilder();
                split = message.msg.ToLower().Split(' ');
                switch (split[0])
                {
                    case ".gold":
                        float gold;
                        if (split.Length < 2)
                            return true;
                        if (float.TryParse(split[1], out gold))
                            game.getPeerInfo(peer).getChampion().getStats().Gold = gold;
                        return true;
                    case ".speed":
                        float speed;
                        if (split.Length < 2)
                            return true;
                        if (float.TryParse(split[1], out speed))
                            game.getPeerInfo(peer).getChampion().getStats().MoveSpeed.BaseValue = speed;
                        PacketNotifier.notifyUpdatedStats(game.getPeerInfo(peer).getChampion(), false);
                        return true;
                    case ".health":
                        float hp;
                        if (split.Length < 2)
                            return true;
                        if (float.TryParse(split[1], out hp))
                        {
                            game.getPeerInfo(peer).getChampion().getStats().CurrentHealth = hp;
                            game.getPeerInfo(peer).getChampion().getStats().HealthPoints.BaseValue = hp;

                            PacketNotifier.notifySetHealth(game.getPeerInfo(peer).getChampion());
                        }
                        return true;
                    case ".xp":
                        float xp;
                        if (split.Length < 2)
                            return true;
                        if (float.TryParse(split[1], out xp))
                            game.getPeerInfo(peer).getChampion().getStats().Experience = xp;
                        PacketNotifier.notifyUpdatedStats(game.getPeerInfo(peer).getChampion(), false);
                        return true;
                    case ".ap":
                        float ap;
                        if (split.Length < 2)
                            return true;
                        if (float.TryParse(split[1], out ap))
                            game.getPeerInfo(peer).getChampion().getStats().AbilityPower.FlatBonus = ap;
                        PacketNotifier.notifyUpdatedStats(game.getPeerInfo(peer).getChampion(), false);
                        return true;
                    case ".ad":
                        float ad;
                        if (split.Length < 2)
                            return true;
                        if (float.TryParse(split[1], out ad))
                            game.getPeerInfo(peer).getChampion().getStats().AttackDamage.FlatBonus = ad;
                        PacketNotifier.notifyUpdatedStats(game.getPeerInfo(peer).getChampion(), false);
                        return true;
                    case ".mana":
                        float mp;
                        if (split.Length < 2)
                            return true;
                        if (float.TryParse(split[1], out mp))
                        {
                            game.getPeerInfo(peer).getChampion().getStats().CurrentMana = mp;
                            game.getPeerInfo(peer).getChampion().getStats().ManaPoints.BaseValue = mp;
                        }
                        PacketNotifier.notifyUpdatedStats(game.getPeerInfo(peer).getChampion(), false);
                        return true;
                    case ".model":
                        if (split.Length >= 2)
                            game.getPeerInfo(peer).getChampion().setModel(split[1]);
                        return true;
                    case ".help":
                        debugMsg.Append("List of available commands: ");
                        foreach (var cc in cmd)
                            debugMsg.Append(cc + " ");

                        var dm = new DebugMessage(debugMsg.ToString());
                        PacketHandlerManager.getInstace().sendPacket(peer, dm, Channel.CHL_S2C);
                        return true;
                    case ".spawn":
                        Logger.LogCoreInfo("Not implemented command .spawn");
                        return true;
                    case ".size":
                        float size;
                        if (split.Length < 2)
                            return true;
                        if (float.TryParse(split[1], out size))
                            game.getPeerInfo(peer).getChampion().getStats().Size.BaseValue = size;
                        return true;
                    case ".junglespawn":
                        cmd = new string[] { "c baron", "c wolves", "c red", "c blue", "c dragon", "c wraiths", "c golems" };
                        return true;
                    case ".skillpoints":
                        game.getPeerInfo(peer).getChampion().setSkillPoints(17);
                        var skillUpResponse = new SkillUpPacket(game.getPeerInfo(peer).getChampion().getNetId(), 0, 0, 17);
                        PacketHandlerManager.getInstace().sendPacket(peer, skillUpResponse, Channel.CHL_GAMEPLAY);
                        return true;
                    case ".level":
                        float lvl;
                        if (split.Length < 2)
                            return true;
                        if (float.TryParse(split[1], out lvl))
                        {
                            if (lvl < 1 || lvl > 18)
                                return true;
                            game.getPeerInfo(peer).getChampion().getStats().Experience = game.getMap().getExperienceToLevelUp()[(int)lvl - 1];
                            //game.peerInfo(peer).getChampion().getStats().setLevel(lvl);
                        }
                        return true;
                    case ".tp":
                        float x, y;
                        if (split.Length < 3)
                            return true;
                        if (float.TryParse(split[1], out x))
                            if (float.TryParse(split[2], out y))
                                PacketNotifier.notifyTeleport(game.getPeerInfo(peer).getChampion(), x, y);
                        return true;
                    case ".coords":
                        Logger.LogCoreInfo("At " + game.getPeerInfo(peer).getChampion().getX() + ";" + game.getPeerInfo(peer).getChampion().getY());
                        debugMsg.Append("At Coords - X: ");
                        debugMsg.Append(game.getPeerInfo(peer).getChampion().getX());
                        debugMsg.Append(" Y: ");
                        debugMsg.Append(game.getPeerInfo(peer).getChampion().getY());
                        debugMsg.Append(" Z: ");
                        debugMsg.Append(game.getPeerInfo(peer).getChampion().getZ());
                        PacketNotifier.notifyDebugMessage(debugMsg.ToString());
                        return true;
                    case ".ch":
                        if (split.Length < 2)
                            return true;
                        new System.Threading.Thread(new System.Threading.ThreadStart(() =>
                        {
                            var c = new Champion(game, split[1], game.getMap(), game.getPeerInfo(peer).getChampion().getNetId(), (uint)game.getPeerInfo(peer).userId);
                            c.setPosition(game.getPeerInfo(peer).getChampion().getX(), game.getPeerInfo(peer).getChampion().getY());
                            c.setModel(split[1]); // trigger the "modelUpdate" proc
                            c.setTeam(game.getPeerInfo(peer).getChampion().getTeam());
                            game.getMap().RemoveObject(game.getPeerInfo(peer).getChampion());
                            game.getMap().AddObject(c);
                            game.getPeerInfo(peer).setChampion(c);
                        })).Start();
                        return true;
                    case ".packet":
                        try
                        {
                            var s = message.msg.Split(' ');
                            if (s.Length < 2)
                                return true;
                            var bytes = new List<byte>();

                            for (var i = 1; i < s.Length; i++)
                            {
                                var ss = s[i].Split(':');
                                var type = ss[0];
                                dynamic num;
                                if (ss[1] == "netid")
                                    num = game.getPeerInfo(peer).getChampion().getNetId();
                                else
                                    num = System.Convert.ChangeType(int.Parse(ss[1]), Type.GetType("System." + type));
                                var d = BitConverter.GetBytes(num);
                                if (num.GetType() == typeof(byte))
                                    bytes.Add(num);
                                else
                                    bytes.AddRange(d);
                            }

                            PacketHandlerManager.getInstace().sendPacket(peer, bytes.ToArray(), Channel.CHL_C2S);
                        }
                        catch { }
                        return true;
                    case ".mobs":
                        if (split.Length < 2)
                            return true;
                        int team;
                        if (!int.TryParse(split[1], out team))
                            return true;
                        var units = game.getPeerInfo(peer).getChampion().getMap().GetObjects().Where(xx => xx.Value.getTeam() == CustomConvert.toTeamId(team)).Where(xx => xx.Value is Minion);
                        foreach (var unit in units)
                        {
                            var response = new AttentionPingAns(game.getPeerInfo(peer), new AttentionPing { x = unit.Value.getX(), y = unit.Value.getY(), targetNetId = 0, type = Pings.Ping_Danger });
                            PacketHandlerManager.getInstace().broadcastPacketTeam(game.getPeerInfo(peer).getTeam(), response, Channel.CHL_S2C);
                        }
                        return true;
                    case ".inhib":
                        var sender = game.getPeerInfo(peer);
                        var min = new Monster(game.getMap(), Game.GetNewNetID(), sender.getChampion().getX(), sender.getChampion().getY(), sender.getChampion().getX(), sender.getChampion().getY(), "AncientGolem", "AncientGolem1.1.1");
                        game.getMap().AddObject(min);
                        return true;
                    case ".reload":
                        game.setStarted(false);
                        foreach (var obj in game.getMap().getObjects())
                        {
                            var m = obj.Value as Minion;
                            if (m != null)
                            {
                                m.LoadLua();
                            }

                            var c = obj.Value as Champion;
                            if (c != null)
                            {
                                c.LoadLua();
                            }
                        }
                        game.setStarted(true);
                        return true;
                }
            }

            #endregion
            switch (message.type)
            {
                case ChatType.CHAT_ALL:
                    return PacketHandlerManager.getInstace().broadcastPacket(data, Channel.CHL_COMMUNICATION);
                case ChatType.CHAT_TEAM:
                    return PacketHandlerManager.getInstace().broadcastPacketTeam(game.getPeerInfo(peer).getTeam(), data, Channel.CHL_COMMUNICATION);
                default:
                    //Logging.errorLine("Unknown ChatMessageType");
                    return PacketHandlerManager.getInstace().sendPacket(peer, data, Channel.CHL_COMMUNICATION);
            }
        }
    }
}
