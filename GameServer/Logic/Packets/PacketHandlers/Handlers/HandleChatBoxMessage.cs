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
    unsafe class HandleChatBoxMessage : IPacketHandler
    {
        static List<ChatCommand> commandsList = new List<ChatCommand>();

        static Dictionary<string, ChatCommand> ChatCommandsDictionary = new Dictionary<string, ChatCommand>();

        class ChatCommand
        { 
            public string command;
            public string syntax;
            public bool hidden;
            public bool disabled;
            public delExecute execute;

            public ChatCommand(string command, string syntax, delExecute function)
            {
                if (ChatCommandsDictionary.ContainsKey(command))
                {
                    return;
                }
                this.command = command;
                this.syntax = syntax;
                this.execute = function;
                commandsList.Add(this);
            }

            public delegate void delExecute(ENetPeer* peer, Game game, bool receivedArguments, string arguments = "");            
        }

        #region Command functions
        public static void adCmdFunction(ENetPeer* peer, Game game, bool receivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            float ad;
            if (split.Length < 2)
            {
                ChatCommand command;
                SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);                
                ChatCommandsDictionary.TryGetValue(split[0], out command);
                SendDebugMsgFormatted(DebugMsgType.SYNTAX, command.syntax); 
            }
            else if (float.TryParse(split[1], out ad))
                game.getPeerInfo(peer).getChampion().getStats().setBonusAdFlat(ad);
        }

        public static void apCmdFunction(ENetPeer* peer, Game game, bool receivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            float ap;
            if (split.Length < 2)
            {
                ChatCommand command;
                SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ChatCommandsDictionary.TryGetValue(split[0], out command);
                SendDebugMsgFormatted(DebugMsgType.SYNTAX, command.syntax);
            }
            else if (float.TryParse(split[1], out ap))
                game.getPeerInfo(peer).getChampion().getStats().setBonusApFlat(ap);
        }

        public static void chCmdFunction(ENetPeer* peer, Game game, bool receivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            if (split.Length < 2)
            {
                ChatCommand command;
                SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ChatCommandsDictionary.TryGetValue(split[0], out command);
                SendDebugMsgFormatted(DebugMsgType.SYNTAX, command.syntax);
            }
            new System.Threading.Thread(new System.Threading.ThreadStart(() =>
            {
                var c = new Champion(game, split[1], game.getMap(), game.getPeerInfo(peer).getChampion().getNetId(), (uint)game.getPeerInfo(peer).userId);
                c.setPosition(game.getPeerInfo(peer).getChampion().getX(), game.getPeerInfo(peer).getChampion().getY());
                c.setModel(split[1]); // trigger the "modelUpdate" proc
                c.setTeam(game.getPeerInfo(peer).getChampion().getTeam());
                game.getMap().removeObject(game.getPeerInfo(peer).getChampion());
                game.getMap().addObject(c);
                game.getPeerInfo(peer).setChampion(c);
            })).Start();
        }

        public static void coordsCmdFunction(ENetPeer* peer, Game game, bool receivedArguments, string arguments = "")
        {
            Logger.LogCoreInfo("At " + game.getPeerInfo(peer).getChampion().getX() + ";" + game.getPeerInfo(peer).getChampion().getY());
            StringBuilder debugMsg = new StringBuilder();
            debugMsg.Append("At Coords - X: ");
            debugMsg.Append(game.getPeerInfo(peer).getChampion().getX());
            debugMsg.Append(" Y: ");
            debugMsg.Append(game.getPeerInfo(peer).getChampion().getY());
            debugMsg.Append(" Z: ");
            debugMsg.Append(game.getPeerInfo(peer).getChampion().getZ());
            SendDebugMsgFormatted(DebugMsgType.NORMAL, debugMsg.ToString());
        }

        public static void goldCmdFunction(ENetPeer* peer, Game game, bool receivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            float gold;
            if (split.Length < 2)
            {
                ChatCommand command;
                SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ChatCommandsDictionary.TryGetValue(split[0], out command);
                SendDebugMsgFormatted(DebugMsgType.SYNTAX, command.syntax);
            }
            else if (float.TryParse(split[1], out gold))
                game.getPeerInfo(peer).getChampion().getStats().setGold(gold);
        }

        public static void healthCmdFunction(ENetPeer* peer, Game game, bool receivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            float hp;
            if (split.Length < 2)
            {
                ChatCommand command;
                SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ChatCommandsDictionary.TryGetValue(split[0], out command);
                SendDebugMsgFormatted(DebugMsgType.SYNTAX, command.syntax);
            }
            else if (float.TryParse(split[1], out hp))
            {
                game.getPeerInfo(peer).getChampion().getStats().setCurrentHealth(hp);
                game.getPeerInfo(peer).getChampion().getStats().setMaxHealth(hp);
            }
        }

        public static void newCommandCmdFunction(ENetPeer* peer, Game game, bool receivedArguments, string arguments = "")
        {
            SendDebugMsgFormatted(DebugMsgType.INFO, "The new command added by .help has been executed");

            foreach (KeyValuePair<string, ChatCommand> keyValuePair in ChatCommandsDictionary)
            {
                if (keyValuePair.Key == ".newcommand")
                {
                    commandsList.Remove(keyValuePair.Value);
                }
            }
        }

        public static void helpCmdFunction(ENetPeer* peer, Game game, bool receivedArguments, string arguments = "")
        {
            string commands = "";
            int count = 0;
            foreach (ChatCommand com in commandsList)
            {
                count += 1;
                commands = commands
                           + "<font color =\"#E175FF\"><b>"
                           + com.command 
                           + "</b><font color =\"#FFB145\">, ";
            }
            SendDebugMsgFormatted(DebugMsgType.INFO, "List of available commands: ");
            SendDebugMsgFormatted(DebugMsgType.INFO, commands);
            SendDebugMsgFormatted(DebugMsgType.INFO, "There are " + count.ToString() + " commands");

            ChatCommand newCommandCmd = new ChatCommand(".newcommand", "", newCommandCmdFunction);
        }

        public static void inhibCmdFunction(ENetPeer* peer, Game game, bool receivedArguments, string arguments = "")
        {
            var sender = game.getPeerInfo(peer);
            var min = new Monster(game.getMap(), Game.GetNewNetID(), sender.getChampion().getX(), sender.getChampion().getY(), sender.getChampion().getX(), sender.getChampion().getY(), "Worm", "Worm");//"AncientGolem", "AncientGolem1.1.1");
            game.getMap().addObject(min);
        }

        public static void junglespawnCmdFunction(ENetPeer* peer, Game game, bool receivedArguments, string arguments = "")
        {
            Logger.LogCoreInfo(".junglespawn command not implemented");
            SendDebugMsgFormatted(DebugMsgType.INFO, "Command not implemented");
        }

        public static void levelCmdFunction(ENetPeer* peer, Game game, bool receivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            float lvl;
            if (split.Length < 2)
            {
                ChatCommand command;
                SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ChatCommandsDictionary.TryGetValue(split[0], out command);
                SendDebugMsgFormatted(DebugMsgType.SYNTAX, command.syntax);
            }
            else if (float.TryParse(split[1], out lvl))
            {
                if (lvl < 1 || lvl > 18)
                    return;
                game.getPeerInfo(peer).getChampion().getStats().setExp(game.getMap().getExperienceToLevelUp()[(int)lvl - 1]);
                //game.peerInfo(peer).getChampion().getStats().setLevel(lvl);
            }
        }

        public static void manaCmdFunction(ENetPeer* peer, Game game, bool receivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            float mp;
            if (split.Length < 2)
            {
                ChatCommand command;
                SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ChatCommandsDictionary.TryGetValue(split[0], out command);
                SendDebugMsgFormatted(DebugMsgType.SYNTAX, command.syntax);
            }
            else if (float.TryParse(split[1], out mp))
            {
                game.getPeerInfo(peer).getChampion().getStats().setCurrentMana(mp);
                game.getPeerInfo(peer).getChampion().getStats().setMaxMana(mp);
            }
        }

        public static void mobsCmdFunction(ENetPeer* peer, Game game, bool receivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            if (split.Length < 2)
            {
                ChatCommand command;
                SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ChatCommandsDictionary.TryGetValue(split[0], out command);
                SendDebugMsgFormatted(DebugMsgType.SYNTAX, command.syntax);
                return;
            }
            int team;
            if (!int.TryParse(split[1], out team))
                return;
            var units = game.getPeerInfo(peer).getChampion().getMap().getObjects().Where(xx => xx.Value.getTeam() == CustomConvert.toTeamId(team)).Where(xx => xx.Value is Minion);
            foreach (var unit in units)
            {
                var response = new AttentionPingAns(game.getPeerInfo(peer), new AttentionPing { x = unit.Value.getX(), y = unit.Value.getY(), targetNetId = 0, type = Pings.Ping_Danger });
                PacketHandlerManager.getInstace().broadcastPacketTeam(game.getPeerInfo(peer).getTeam(), response, Channel.CHL_S2C);
            }
        }

        public static void modelCmdFunction(ENetPeer* peer, Game game, bool receivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            if (split.Length >= 2)
                game.getPeerInfo(peer).getChampion().setModel(split[1]);
            else
            {
                ChatCommand command;
                SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ChatCommandsDictionary.TryGetValue(split[0], out command);
                SendDebugMsgFormatted(DebugMsgType.SYNTAX, command.syntax);
            }
        }

        public static void packetCmdFunction(ENetPeer* peer, Game game, bool receivedArguments, string arguments = "")
        {
            try
            {
                var s = arguments.Split(' ');
                if (s.Length < 2)
                {
                    ChatCommand command;
                    SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                    ChatCommandsDictionary.TryGetValue(s[0], out command);
                    SendDebugMsgFormatted(DebugMsgType.SYNTAX, command.syntax);
                    return;
                }
                    
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
        }

        public static void setCmdFunction(ENetPeer* peer, Game game, bool receivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            if (split.Length < 4)
            {
                ChatCommand command;
                SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ChatCommandsDictionary.TryGetValue(split[0], out command);
                SendDebugMsgFormatted(DebugMsgType.SYNTAX, command.syntax);
                return;
            }
            int blockNo, fieldNo = 0;
            float value = 0;
            if (int.TryParse(split[1], out blockNo))
                if (int.TryParse(split[2], out fieldNo))
                    if (float.TryParse(split[3], out value))
                    {
                        // blockNo = 1 << (blockNo - 1);
                        //var mask = 1 << (fieldNo - 1);
                        game.getPeerInfo(peer).getChampion().getStats().setStat((MasterMask)blockNo, (FieldMask)fieldNo, value);
                    }
        }

        public static void sizeCmdFunction(ENetPeer* peer, Game game, bool receivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            float size;
            if (split.Length < 2)
            {
                ChatCommand command;
                SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ChatCommandsDictionary.TryGetValue(split[0], out command);
                SendDebugMsgFormatted(DebugMsgType.SYNTAX, command.syntax);
                return;
            }
            else if (float.TryParse(split[1], out size))
                game.getPeerInfo(peer).getChampion().getStats().setSize(size);
        }

        public static void skillpointsCmdFunction(ENetPeer* peer, Game game, bool receivedArguments, string arguments = "")
        {
            game.getPeerInfo(peer).getChampion().setSkillPoints(17);
            var skillUpResponse = new SkillUpPacket(game.getPeerInfo(peer).getChampion().getNetId(), 0, 0, 17);
            PacketHandlerManager.getInstace().sendPacket(peer, skillUpResponse, Channel.CHL_GAMEPLAY);
        }

        public static void spawnCmdFunction(ENetPeer* peer, Game game, bool receivedArguments, string arguments = "")
        {
            Logger.LogCoreInfo(".spawn command not implemented");
            SendDebugMsgFormatted(DebugMsgType.INFO, "Command not implemented");
        }

        public static void speedCmdFunction(ENetPeer* peer, Game game, bool receivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            float speed;
            if (split.Length < 2)
            {
                ChatCommand command;
                SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ChatCommandsDictionary.TryGetValue(split[0], out command);
                SendDebugMsgFormatted(DebugMsgType.SYNTAX, command.syntax);
            }
            if (float.TryParse(split[1], out speed))
                game.getPeerInfo(peer).getChampion().getStats().setMovementSpeed(speed);
            else
                SendDebugMsgFormatted(DebugMsgType.ERROR, "Incorrect parameter");
        }

        public static void tpCmdFunction(ENetPeer* peer, Game game, bool receivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            float x, y;
            if (split.Length < 3)
            {
                ChatCommand command;
                SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ChatCommandsDictionary.TryGetValue(split[0], out command);
                SendDebugMsgFormatted(DebugMsgType.SYNTAX, command.syntax);
            }
            if (float.TryParse(split[1], out x))
                if (float.TryParse(split[2], out y))
                    PacketNotifier.notifyTeleport(game.getPeerInfo(peer).getChampion(), x, y);
        }

        public static void xpCmdFunction(ENetPeer* peer, Game game, bool receivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            float xp;
            if (split.Length < 2)
            {
                ChatCommand command;
                SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                ChatCommandsDictionary.TryGetValue(split[0], out command);
                SendDebugMsgFormatted(DebugMsgType.SYNTAX, command.syntax);
            }
            if (float.TryParse(split[1], out xp))
                game.getPeerInfo(peer).getChampion().getStats().setExp(xp);
        }
        #endregion

        #region Add commands here:
        ChatCommand adCmd = new ChatCommand(".ad", ".ad bonusAd", adCmdFunction);
        ChatCommand apCmd = new ChatCommand(".ad", ".ap bonusAp", apCmdFunction);
        ChatCommand chCmd = new ChatCommand(".ch", ".ch championName", chCmdFunction);
        ChatCommand coordsCmd = new ChatCommand(".coords", "", coordsCmdFunction);
        ChatCommand goldCmd = new ChatCommand(".gold", ".gold goldAmount", goldCmdFunction);
        ChatCommand healthCmd = new ChatCommand(".health", ".health maxHealth", healthCmdFunction);
        ChatCommand helpCmd = new ChatCommand(".help", "", helpCmdFunction);
        ChatCommand inhibCmd = new ChatCommand(".inhib", "", inhibCmdFunction);
        ChatCommand junglespawnCmd = new ChatCommand(".junglespawn", "", junglespawnCmdFunction);
        ChatCommand levelCmd = new ChatCommand(".level", ".level level", levelCmdFunction);
        ChatCommand manaCmd = new ChatCommand(".mana", ".mana maxMana", manaCmdFunction);
        ChatCommand mobsCmd = new ChatCommand(".mobs", ".mobs teamNumber", mobsCmdFunction);
        ChatCommand modelCmd = new ChatCommand(".model", ".model modelName", modelCmdFunction);
        ChatCommand packetCmd = new ChatCommand(".packet", "No idea, too lazy to read the code", packetCmdFunction);
        ChatCommand setCmd = new ChatCommand(".set", ".set masterMask fieldMask", setCmdFunction);
        ChatCommand sizeCmd = new ChatCommand(".size", ".size size", sizeCmdFunction);
        ChatCommand skillpointsCmd = new ChatCommand(".skillpoints", "", skillpointsCmdFunction);
        ChatCommand spawnCmd = new ChatCommand(".spawn", "", spawnCmdFunction);
        ChatCommand speedCmd = new ChatCommand(".speed", ".speed speed", speedCmdFunction);
        ChatCommand tpCmd = new ChatCommand(".tp", ".tp x y", tpCmdFunction);
        ChatCommand xpCmd = new ChatCommand(".xp", ".xp xp", xpCmdFunction);
        #endregion

        public enum DebugMsgType {ERROR, INFO, SYNTAX, SYNTAXERROR, NORMAL};

        public static void SendDebugMsgFormatted(DebugMsgType type, string message = "")
        {   
            var formattedText = new StringBuilder();
            int fontSize = 20; // Big fonts seem to make the chatbox buggy
                               // This may need to be removed.
            switch (type)
            {
                case DebugMsgType.ERROR: // Tag: [ERROR], Color: Red
                    formattedText.Append("<font size=\"" + fontSize + "\" color =\"#FF0000\"><b>[ERROR]</b><font color =\"#AFBF00\">: ");
                    formattedText.Append(message);
                    PacketNotifier.notifyDebugMessage(formattedText.ToString());
                    break;
                case DebugMsgType.INFO: // Tag: [INFO], Color: Green
                    formattedText.Append("<font size=\"" + fontSize + "\" color =\"#00D90E\"><b>[INFO]</b><font color =\"#AFBF00\">: ");
                    formattedText.Append(message);
                    PacketNotifier.notifyDebugMessage(formattedText.ToString());
                    break;
                case DebugMsgType.SYNTAX: // Tag: [SYNTAX], Color: Blue
                    formattedText.Append("<font size=\"" + fontSize + "\" color =\"#006EFF\"><b>[SYNTAX]</b><font color =\"#AFBF00\">: ");
                    formattedText.Append(message);
                    PacketNotifier.notifyDebugMessage(formattedText.ToString());
                    break;
                case DebugMsgType.SYNTAXERROR: // Tag: [ERROR], Color: Red
                    formattedText.Append("<font size=\"" + fontSize + "\" color =\"#FF0000\"><b>[ERROR]</b><font color =\"#AFBF00\">: ");
                    formattedText.Append("Incorrect command syntax");
                    PacketNotifier.notifyDebugMessage(formattedText.ToString());
                    break;
                case DebugMsgType.NORMAL: // No tag, no format
                    PacketNotifier.notifyDebugMessage(message);
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
            // Load commands
            ChatCommandsDictionary.Clear();
            foreach (ChatCommand com in commandsList) {
                if (!ChatCommandsDictionary.ContainsKey(com.command)) {
                    ChatCommandsDictionary.Add(com.command, com);
                }
            }
            // Execute commands
            if (message.msg.StartsWith("."))
            {
                split = message.msg.ToLower().Split(' ');

                ChatCommand command;
                if (ChatCommandsDictionary.TryGetValue(split[0], out command))
                {
                    if (command.hidden) // This could also check if the user has access to hidden commands
                    {
                        SendDebugMsgFormatted(DebugMsgType.ERROR, "<font color =\"#E175FF\"><b>" + split[0] + "</b><font color =\"#AFBF00\"> is not a valid command.");
                        SendDebugMsgFormatted(DebugMsgType.INFO, "Type <font color =\"#E175FF\"><b>.help</b><font color =\"#AFBF00\"> for a list of available commands");
                        return true;
                    }
                    else if (command.disabled)
                    {
                        SendDebugMsgFormatted(DebugMsgType.ERROR, "<font color =\"#E175FF\"><b>" + split[0] + "</b><font color =\"#AFBF00\"> is disabled.");
                        return true;
                    }
                    command.execute(peer, game, true, message.msg);
                    return true;
                }
                else
                {
                    SendDebugMsgFormatted(DebugMsgType.ERROR, "<font color =\"#E175FF\"><b>" + split[0] + "</b><font color =\"#AFBF00\"> is not a valid command.");
                    SendDebugMsgFormatted(DebugMsgType.INFO, "Type <font color =\"#E175FF\"><b>.help</b><font color =\"#AFBF00\"> for a list of available commands");
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
