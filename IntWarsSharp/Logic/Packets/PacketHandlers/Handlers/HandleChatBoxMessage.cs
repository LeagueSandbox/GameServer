using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using static ENet.Native;
using IntWarsSharp.Logic.Packets;
using IntWarsSharp.Logic.GameObjects;

namespace IntWarsSharp.Core.Logic.PacketHandlers.Packets
{
    class HandleChatBoxMessage : IPacketHandler
    {
        public unsafe bool HandlePacket(ENetPeer* peer, byte[] data, Game game)
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
                        var response = new AttentionPingAns(game.peerInfo(peer), new AttentionPing { x = x, y = y, targetNetId = 0, type = 0 });
                        PacketHandlerManager.getInstace().broadcastPacketTeam(game.peerInfo(peer).getTeam(), response, Channel.CHL_S2C);
                    }
                }
            }

            #region Commands
            //Lets do commands

            if (message.msg.StartsWith("."))
            {
                var cmd = new string[] { ".set", ".gold", ".speed", ".health", ".xp", ".ap", ".ad", ".mana", ".model", ".help", ".spawn", ".size", ".junglespawn", ".skillpoints", ".level", ".tp", ".coords", ".ch" };
                var debugMsg = new StringBuilder();
                split = message.msg.ToLower().Split(' ');
                switch (split[0])
                {
                    case ".set":
                        if (split.Length < 4)
                            return true;
                        int blockNo, fieldNo = 0;
                        float value = 0;
                        if (int.TryParse(split[1], out blockNo))
                            if (int.TryParse(split[2], out fieldNo))
                                if (float.TryParse(split[3], out value))
                                {
                                    blockNo = 1 << (blockNo - 1);
                                    var mask = 1 << (fieldNo - 1);
                                    game.peerInfo(peer).getChampion().getStats().setStat((MasterMask)blockNo, (FieldMask)mask, value);
                                }
                        return true;
                    case ".gold":
                        float gold;
                        if (split.Length < 2)
                            return true;
                        if (float.TryParse(split[1], out gold))
                            game.peerInfo(peer).getChampion().getStats().setGold(gold);
                        return true;
                    case ".speed":
                        float speed;
                        if (split.Length < 2)
                            return true;
                        if (float.TryParse(split[1], out speed))
                            game.peerInfo(peer).getChampion().getStats().setMovementSpeed(speed);
                        return true;
                    case ".health":
                        float hp;
                        if (split.Length < 2)
                            return true;
                        if (float.TryParse(split[1], out hp))
                        {
                            game.peerInfo(peer).getChampion().getStats().setCurrentHealth(hp);
                            game.peerInfo(peer).getChampion().getStats().setMaxHealth(hp);

                            PacketNotifier.notifySetHealth(game.peerInfo(peer).getChampion());
                        }
                        return true;
                    case ".xp":
                        float xp;
                        if (split.Length < 2)
                            return true;
                        if (float.TryParse(split[1], out xp))
                            game.peerInfo(peer).getChampion().getStats().setExp(xp);
                        return true;
                    case ".ap":
                        float ap;
                        if (split.Length < 2)
                            return true;
                        if (float.TryParse(split[1], out ap))
                            game.peerInfo(peer).getChampion().getStats().setBonusApFlat(ap);
                        return true;
                    case ".ad":
                        float ad;
                        if (split.Length < 2)
                            return true;
                        if (float.TryParse(split[1], out ad))
                            game.peerInfo(peer).getChampion().getStats().setBonusAdFlat(ad);
                        return true;
                    case ".mana":
                        float mp;
                        if (split.Length < 2)
                            return true;
                        if (float.TryParse(split[1], out mp))
                        {
                            game.peerInfo(peer).getChampion().getStats().setCurrentMana(mp);
                            game.peerInfo(peer).getChampion().getStats().setMaxMana(mp);
                        }
                        return true;
                    case ".model":
                        if (split.Length >= 2)
                            game.peerInfo(peer).getChampion().setModel(split[1]);
                        return true;
                    case ".help":
                        debugMsg.Append("List of available commands: ");
                        foreach (var cc in cmd)
                            debugMsg.Append(cc + " ");

                        var dm = new SpawnParticle.DebugMessage(debugMsg.ToString());
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
                            game.peerInfo(peer).getChampion().getStats().setSize(size);
                        return true;
                    case ".junglespawn":
                        cmd = new string[] { "c baron", "c wolves", "c red", "c blue", "c dragon", "c wraiths", "c golems" };
                        return true;
                    case ".skillpoints":
                        game.peerInfo(peer).getChampion().setSkillPoints(17);
                        var skillUpResponse = new SkillUpResponse(game.peerInfo(peer).getChampion().getNetId(), 0, 0, 17);
                        PacketHandlerManager.getInstace().sendPacket(peer, skillUpResponse, Channel.CHL_GAMEPLAY);
                        return true;
                    case ".level":
                        float lvl;
                        if (split.Length < 2)
                            return true;
                        if (float.TryParse(split[1], out lvl))
                            game.peerInfo(peer).getChampion().getStats().setLevel(lvl);
                        return true;
                    case ".tp":
                        float x, y;
                        if (split.Length < 3)
                            return true;
                        if (float.TryParse(split[1], out x))
                            if (float.TryParse(split[2], out y))
                                PacketNotifier.notifyTeleport(game.peerInfo(peer).getChampion(), x, y);
                        return true;
                    case ".coords":
                        Logger.LogCoreInfo("At " + game.peerInfo(peer).getChampion().getX() + ";" + game.peerInfo(peer).getChampion().getY());
                        debugMsg.Append("At Coords - X: ");
                        debugMsg.Append(game.peerInfo(peer).getChampion().getX());
                        debugMsg.Append(" Y: ");
                        debugMsg.Append(game.peerInfo(peer).getChampion().getY());
                        debugMsg.Append(" Z: ");
                        debugMsg.Append(game.peerInfo(peer).getChampion().getZ());
                        PacketNotifier.notifyDebugMessage(debugMsg.ToString());
                        return true;
                    case ".ch":
                        if (split.Length < 2)
                            return true;
                        var c = new Champion(split[1], game.getMap(), game.peerInfo(peer).getChampion().getNetId(), (int)game.peerInfo(peer).userId);
                        c.setPosition(game.peerInfo(peer).getChampion().getX(), game.peerInfo(peer).getChampion().getY());
                        c.setModel(split[1]); // trigger the "modelUpdate" proc
                        game.getMap().removeObject(game.peerInfo(peer).getChampion());
                        game.getMap().addObject(c);
                        game.peerInfo(peer).setChampion(c);
                        return true;
                }
            }

            #endregion
            switch (message.type)
            {
                case ChatType.CHAT_ALL:
                    return PacketHandlerManager.getInstace().broadcastPacket(data, Channel.CHL_COMMUNICATION);
                case ChatType.CHAT_TEAM:
                    return PacketHandlerManager.getInstace().broadcastPacketTeam(game.peerInfo(peer).getTeam(), data, Channel.CHL_COMMUNICATION);
                default:
                    //Logging.errorLine("Unknown ChatMessageType");
                    return PacketHandlerManager.getInstace().sendPacket(peer, data, Channel.CHL_COMMUNICATION);
            }
        }
    }
}
