using System;
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
        void SendDebugMsgFormatted(int type, string message)
        {
            var formattedText = new StringBuilder();
            int fontSize = 20;
            switch (type)
            {
                case 0:
                    formattedText.Append("<font size=\"" + fontSize + "\" color =\"#FF0000\"><b>[ERROR]</b><font color =\"#AFBF00\">: ");
                    formattedText.Append(message);
                    PacketNotifier.notifyDebugMessage(formattedText.ToString());
                    break;
                case 1:
                    formattedText.Append("<font size=\"" + fontSize + "\" color =\"#00D90E\"><b>[INFO]</b><font color =\"#AFBF00\">: ");
                    formattedText.Append(message);
                    PacketNotifier.notifyDebugMessage(formattedText.ToString());
                    break;
                case 2:
                    formattedText.Append("<font size=\"" + fontSize + "\" color =\"#006EFF\"><b>[SYNTAX]</b><font color =\"#AFBF00\">: ");
                    formattedText.Append(message);
                    PacketNotifier.notifyDebugMessage(formattedText.ToString());
                    break;
            }
        }

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
                        var response = new AttentionPingAns(game.GetPeerInfo(peer), new AttentionPing { x = x, y = y, targetNetId = 0, type = 0 });
                        game.PacketHandlerManager.broadcastPacketTeam(game.GetPeerInfo(peer).GetTeam(), response, Channel.CHL_S2C);
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
                        {
                            SendDebugMsgFormatted(0, "Incorrect command syntax");                            
                            SendDebugMsgFormatted(2, ".gold goldAmount"); // It might be better to replace this
                            return true;                                  // in the future with something like
                        }                                                 // ShowCommandSyntax();
                            
                        if (float.TryParse(split[1], out gold))
                            game.GetPeerInfo(peer).GetChampion().GetStats().Gold = gold;
                        return true;
                    case ".speed":
                        float speed;
                        if (split.Length < 2)
                        {
                            SendDebugMsgFormatted(0, "Incorrect command syntax");
                            SendDebugMsgFormatted(2, ".speed movementSpeed");
                            return true;
                        }
                        if (float.TryParse(split[1], out speed))
                            game.GetPeerInfo(peer).GetChampion().GetStats().MoveSpeed.BaseValue = speed;
                        return true;
                    case ".health":
                        float hp;
                        if (split.Length < 2)
                        {
                            SendDebugMsgFormatted(0, "Incorrect command syntax");
                            SendDebugMsgFormatted(2, ".health healthAmount");
                            return true;
                        }
                        if (float.TryParse(split[1], out hp))
                        {
                            game.GetPeerInfo(peer).GetChampion().GetStats().CurrentHealth = hp;
                            game.GetPeerInfo(peer).GetChampion().GetStats().HealthPoints.BaseValue = hp;

                            game.PacketNotifier.notifySetHealth(game.GetPeerInfo(peer).GetChampion());
                        }
                        return true;
                    case ".xp":
                        float xp;
                        if (split.Length < 2)
                        {
                            SendDebugMsgFormatted(0, "Incorrect command syntax");
                            SendDebugMsgFormatted(2, ".xp xpAmount");
                            return true;
                        }
                        if (float.TryParse(split[1], out xp))
                            game.GetPeerInfo(peer).GetChampion().GetStats().Experience = xp;
                        return true;
                    case ".ap":
                        float ap;
                        if (split.Length < 2)
                        {
                            SendDebugMsgFormatted(0, "Incorrect command syntax");
                            SendDebugMsgFormatted(2, ".ap apAmount");
                            return true;
                        }
                        if (float.TryParse(split[1], out ap))
                            game.GetPeerInfo(peer).GetChampion().GetStats().AbilityPower.FlatBonus = ap;
                        return true;
                    case ".ad":
                        float ad;
                        if (split.Length < 2)
                        {
                            SendDebugMsgFormatted(0, "Incorrect command syntax");
                            SendDebugMsgFormatted(2, ".ad adAmount");
                            return true;
                        }
                        if (float.TryParse(split[1], out ad))
                            game.GetPeerInfo(peer).GetChampion().GetStats().AttackDamage.FlatBonus = ad;
                        return true;
                    case ".mana":
                        float mp;
                        if (split.Length < 2)
                        {
                            SendDebugMsgFormatted(0, "Incorrect command syntax");
                            SendDebugMsgFormatted(2, ".mana manaAmount");
                            return true;
                        }
                        if (float.TryParse(split[1], out mp))
                        {
                            game.GetPeerInfo(peer).GetChampion().GetStats().CurrentMana = mp;
                            game.GetPeerInfo(peer).GetChampion().GetStats().ManaPoints.BaseValue = mp;
                        }
                        return true;
                    case ".model":
                        if (split.Length >= 2)
                            game.getPeerInfo(peer).getChampion().setModel(split[1]);
                        else
                        {
                            SendDebugMsgFormatted(0, "Incorrect command syntax");
                            SendDebugMsgFormatted(2, ".model modelName");
                            return true;
                        }
                        return true;
                    case ".help":
                        SendDebugMsgFormatted(1, "List of available commands: ");
                        foreach (var cc in cmd)
                            debugMsg.Append("<font color =\"#E175FF\"><br>" + cc + "</br><font color =\"#FFB145\">, ");

                        var dm = new DebugMessage(debugMsg.ToString());
                        game.PacketHandlerManager.sendPacket(peer, dm, Channel.CHL_S2C);
                        return true;
                    case ".spawn":
                        Logger.LogCoreInfo("Not implemented command .spawn");
                        return true;
                    case ".size":
                        float size;
                        if (split.Length < 2)
                        {
                            SendDebugMsgFormatted(0, "Incorrect command syntax");
                            SendDebugMsgFormatted(2, ".size size");
                            return true;
                        }
                        if (float.TryParse(split[1], out size))
                            game.GetPeerInfo(peer).GetChampion().GetStats().Size.BaseValue = size;
                        return true;
                    case ".junglespawn":
                        cmd = new string[] { "c baron", "c wolves", "c red", "c blue", "c dragon", "c wraiths", "c golems" };
                        return true;
                    case ".skillpoints":
                        game.GetPeerInfo(peer).GetChampion().setSkillPoints(17);
                        var skillUpResponse = new SkillUpPacket(game.GetPeerInfo(peer).GetChampion().getNetId(), 0, 0, 17);
                        game.PacketHandlerManager.sendPacket(peer, skillUpResponse, Channel.CHL_GAMEPLAY);
                        return true;
                    case ".level":
                        float lvl;
                        if (split.Length < 2)
                        {
                            SendDebugMsgFormatted(0, "Incorrect command syntax");
                            SendDebugMsgFormatted(2, ".level level");
                            return true;
                        }
                        if (float.TryParse(split[1], out lvl))
                        {
                            if (lvl < 1 || lvl > 18)
                                return true;
                            game.GetPeerInfo(peer).GetChampion().GetStats().Experience = game.GetMap().GetExperienceToLevelUp()[(int)lvl - 1];
                            //game.peerInfo(peer).getChampion().GetStats().setLevel(lvl);
                        }
                        return true;
                    case ".tp":
                        float x, y;
                        if (split.Length < 3)
                        {
                            SendDebugMsgFormatted(0, "Incorrect command syntax");
                            SendDebugMsgFormatted(2, ".tp x y");
                            return true;
                        }
                        if (float.TryParse(split[1], out x))
                            if (float.TryParse(split[2], out y))
                                game.PacketNotifier.notifyTeleport(game.GetPeerInfo(peer).GetChampion(), x, y);
                        return true;
                    case ".coords":
                        Logger.LogCoreInfo("At " + game.GetPeerInfo(peer).GetChampion().getX() + ";" + game.GetPeerInfo(peer).GetChampion().getY());
                        debugMsg.Append("At Coords - X: ");
                        debugMsg.Append(game.GetPeerInfo(peer).GetChampion().getX());
                        debugMsg.Append(" Y: ");
                        debugMsg.Append(game.GetPeerInfo(peer).GetChampion().getY());
                        debugMsg.Append(" Z: ");
                        debugMsg.Append(game.GetPeerInfo(peer).GetChampion().GetZ());
                        game.PacketNotifier.notifyDebugMessage(debugMsg.ToString());
                        return true;
                    case ".ch":
                        if (split.Length < 2)
                        {
                            SendDebugMsgFormatted(0, "Incorrect command syntax");
                            SendDebugMsgFormatted(2, ".ch championName");
                            return true;
                        }
                        new System.Threading.Thread(new System.Threading.ThreadStart(() =>
                        {
                            var c = new Champion(game, split[1], game.GetPeerInfo(peer).GetChampion().getNetId(), (uint)game.GetPeerInfo(peer).UserId);
                            c.setPosition(game.GetPeerInfo(peer).GetChampion().getX(), game.GetPeerInfo(peer).GetChampion().getY());
                            c.setModel(split[1]); // trigger the "modelUpdate" proc
                            c.setTeam(game.GetPeerInfo(peer).GetChampion().getTeam());
                            game.GetMap().RemoveObject(game.GetPeerInfo(peer).GetChampion());
                            game.GetMap().AddObject(c);
                            game.GetPeerInfo(peer).SetChampion(c);
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
                                    num = game.GetPeerInfo(peer).GetChampion().getNetId();
                                else
                                    num = System.Convert.ChangeType(int.Parse(ss[1]), Type.GetType("System." + type));
                                var d = BitConverter.GetBytes(num);
                                if (num.GetType() == typeof(byte))
                                    bytes.Add(num);
                                else
                                    bytes.AddRange(d);
                            }

                            game.PacketHandlerManager.sendPacket(peer, bytes.ToArray(), Channel.CHL_C2S);
                        }
                        catch { }
                        return true;
                    case ".mobs":
                        if (split.Length < 2)
                            return true;
                        int team;
                        if (!int.TryParse(split[1], out team))
                            return true;
                        var units = game.GetPeerInfo(peer).GetChampion().GetGame().GetMap().GetObjects().Where(xx => xx.Value.getTeam() == CustomConvert.toTeamId(team)).Where(xx => xx.Value is Minion);
                        foreach (var unit in units)
                        {
                            var response = new AttentionPingAns(game.GetPeerInfo(peer), new AttentionPing { x = unit.Value.getX(), y = unit.Value.getY(), targetNetId = 0, type = Pings.Ping_Danger });
                            game.PacketHandlerManager.broadcastPacketTeam(game.GetPeerInfo(peer).GetTeam(), response, Channel.CHL_S2C);
                        }
                        return true;
                    case ".inhib":
                        var sender = game.GetPeerInfo(peer);
                        var min = new Monster(game, game.GetNewNetID(), sender.GetChampion().getX(), sender.GetChampion().getY(), sender.GetChampion().getX(), sender.GetChampion().getY(), "AncientGolem", "AncientGolem1.1.1");
                        game.GetMap().AddObject(min);
                        return true;
                    case ".reload":
                        game.SetStarted(false);
                        foreach (var obj in game.GetMap().GetObjects())
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
                        game.SetStarted(true);
                        return true;
                    default:
                        SendDebugMsgFormatted(0, "<font color =\"#E175FF\"><b>" + split[0] + "</b><font color =\"#AFBF00\"> is not a valid command.");
                        SendDebugMsgFormatted(1, "Type <font color =\"#E175FF\"><b>.help</b><font color =\"#AFBF00\"> for a list of available commands");                       
                        return true;
                }
            }

            #endregion
            switch (message.type)
            {
                case ChatType.CHAT_ALL:
                    return game.PacketHandlerManager.broadcastPacket(data, Channel.CHL_COMMUNICATION);
                case ChatType.CHAT_TEAM:
                    return game.PacketHandlerManager.broadcastPacketTeam(game.GetPeerInfo(peer).GetTeam(), data, Channel.CHL_COMMUNICATION);
                default:
                    //Logging.errorLine("Unknown ChatMessageType");
                    return game.PacketHandlerManager.sendPacket(peer, data, Channel.CHL_COMMUNICATION);
            }
        }
    }
}
