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
        static SortedDictionary<string, ChatCommand> ChatCommandsDictionary = new SortedDictionary<string, ChatCommand>();
        /*{
            {".ad", new AdCommand(".ad", ".ad bonusAd")},
            {".ap", new ApCommand(".ap", ".ap bonusAp")},
            {".ch",  new ChCommand(".ch", ".ch championName")},
            {".coords",  new CoordsCommand(".coords", "")},
            {".gold",  new GoldCommand(".gold", ".gold goldAmount")},
            {".health",  new HealthCommand(".health", ".health maxHealth")},
            {".help",  new HelpCommand(".help", "")},
            {".inhib",  new InhibCommand(".inhib", "")},
            {".junglespawn",  new JunglespawnCommand(".junglespawn", "")},
            {".level",  new LevelCommand(".level", ".level level")},
            {".mana",  new ManaCommand(".mana", ".mana maxMana")},
            {".mobs",  new MobsCommand(".mobs", ".mobs teamNumber")},
            {".model",  new ModelCommand(".model", ".model modelName")},
            {".packet",  new PacketCommand(".packet", "No idea, too lazy to read the code")},
            {".set",  new SetCommand(".set", ".set masterMask fieldMask")},
            {".size",  new SizeCommand(".size", ".size size")},
            {".skillpoints",  new SkillpointsCommand(".skillpoints", "") },
            {".spawn",  new SpawnCommand(".spawn", "")},
            {".speed",  new SpeedCommand(".speed", ".speed speed")},
            {".tp",  new TpCommand(".tp", ".tp x y")},
            {".xp",  new XpCommand(".xp", ".xp xp")}
        };*/

        abstract class ChatCommand
        {
            public string Command { get; set; }
            public string Syntax { get; set; }
            public bool IsHidden { get; set; }
            public bool IsDisabled { get; set; }

            public ChatCommand(string command, string syntax)
            {
                if (ChatCommandsDictionary.ContainsKey(command))
                {
                    Logger.LogCoreInfo("The command \"" + command + "\"" + " already exists in the chat commands dictionary, won't be added again.");
                    return;
                }
                this.Command = command;
                this.Syntax = syntax;
                ChatCommandsDictionary.Add(command, this);
            }

            public abstract void Execute(ENetPeer* peer, Game game, bool hasReceivedArguments, string arguments = "");

            public void ShowSyntax() {
                SendDebugMsgFormatted(DebugMsgType.SYNTAX, this.Syntax);
            }
        }

        #region Command classes
        class AdCommand : ChatCommand
        {
            public AdCommand(string command, string syntax) : base(command, syntax) { }

            public override unsafe void Execute(ENetPeer* peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                var split = arguments.ToLower().Split(' ');
                float ad;
                if (split.Length < 2)
                {
                    SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                    this.ShowSyntax();
                }
                else if (float.TryParse(split[1], out ad))
                    game.getPeerInfo(peer).getChampion().getStats().setBonusAdFlat(ad);
            }
        }

        class ApCommand : ChatCommand
        {
            public ApCommand(string command, string syntax) : base(command, syntax) { }

            public override unsafe void Execute(ENetPeer* peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                var split = arguments.ToLower().Split(' ');
                float ap;
                if (split.Length < 2)
                {
                    SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                    this.ShowSyntax();
                }
                else if (float.TryParse(split[1], out ap))
                    game.getPeerInfo(peer).getChampion().getStats().setBonusApFlat(ap);
            }
        }

        class ChCommand : ChatCommand
        {
            public ChCommand(string command, string syntax) : base(command, syntax) { }

            public override unsafe void Execute(ENetPeer* peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                var split = arguments.ToLower().Split(' ');
                if (split.Length < 2)
                {
                    SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                    this.ShowSyntax();
                    return;
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
        }

        class CoordsCommand : ChatCommand
        {
            public CoordsCommand(string command, string syntax) : base(command, syntax) { }

            public override unsafe void Execute(ENetPeer* peer, Game game, bool hasReceivedArguments, string arguments = "")
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
        }

        class GoldCommand : ChatCommand
        {
            public GoldCommand(string command, string syntax) : base(command, syntax) { }

            public override unsafe void Execute(ENetPeer* peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                var split = arguments.ToLower().Split(' ');
                float gold;
                if (split.Length < 2)
                {
                    SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                    this.ShowSyntax();
                }
                else if (float.TryParse(split[1], out gold))
                    game.getPeerInfo(peer).getChampion().getStats().setGold(gold);
            }
        }

        class HealthCommand : ChatCommand
        {
            public HealthCommand(string command, string syntax) : base(command, syntax) { }

            public override unsafe void Execute(ENetPeer* peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                var split = arguments.ToLower().Split(' ');
                float hp;
                if (split.Length < 2)
                {
                    SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                    this.ShowSyntax();
                }
                else if (float.TryParse(split[1], out hp))
                {
                    game.getPeerInfo(peer).getChampion().getStats().setCurrentHealth(hp);
                    game.getPeerInfo(peer).getChampion().getStats().setMaxHealth(hp);
                }
            }
        }

        class NewCommand : ChatCommand
        {
            public NewCommand(string command, string syntax) : base(command, syntax) { }

            public override unsafe void Execute(ENetPeer* peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                var packet = new GameServer.Logic.Packets.Packet((PacketCmdS2C)0x19);
                var netid = game.getPeerInfo(peer).getChampion().getNetId();

                packet.getBuffer().Write((Int32)netid);
                packet.getBuffer().Write((byte)00);
                packet.getBuffer().Write((Int32)netid);
                packet.getBuffer().Write((string)"Holi");
                packet.getBuffer().Write((byte)00);

                PacketHandlerManager.getInstace().sendPacket(peer, packet, Channel.CHL_S2C);
                SendDebugMsgFormatted(DebugMsgType.INFO, "The new command added by .help has been executed");
                ChatCommandsDictionary.Remove(this.Command);
            }
        }

            class HelpCommand : ChatCommand
        {
            public HelpCommand(string command, string syntax) : base(command, syntax) { }

            public override unsafe void Execute(ENetPeer* peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                string commands = "";
                int count = 0;
                foreach (KeyValuePair<string, ChatCommand> keyValuePair in ChatCommandsDictionary)
                {
                    if (!(keyValuePair.Value.IsHidden || keyValuePair.Value.IsDisabled)) // We could also show disabled commands
                    {                                  // (with a different color, saying they're disabled, etc.)
                        count += 1;
                        commands = commands
                                   + "<font color =\"#E175FF\"><b>"
                                   + keyValuePair.Value.Command
                                   + "</b><font color =\"#FFB145\">, ";
                    }
                }
                SendDebugMsgFormatted(DebugMsgType.INFO, "List of available commands: ");
                SendDebugMsgFormatted(DebugMsgType.INFO, commands);
                SendDebugMsgFormatted(DebugMsgType.INFO, "There are " + count.ToString() + " commands");

                var NewCommandCmd = new NewCommand(".newcommand", "");
            }
        }

        class InhibCommand : ChatCommand
        {
            public InhibCommand(string command, string syntax) : base(command, syntax) { }

            public override unsafe void Execute(ENetPeer* peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                var sender = game.getPeerInfo(peer);
                var min = new Monster(game.getMap(), Game.GetNewNetID(), sender.getChampion().getX(), sender.getChampion().getY(), sender.getChampion().getX(), sender.getChampion().getY(), "Worm", "Worm");//"AncientGolem", "AncientGolem1.1.1");
                game.getMap().addObject(min);
            }
        }

        class JunglespawnCommand : ChatCommand
        {
            public JunglespawnCommand(string command, string syntax) : base(command, syntax) { }

            public override unsafe void Execute(ENetPeer* peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                Logger.LogCoreInfo(".junglespawn command not implemented");
                SendDebugMsgFormatted(DebugMsgType.INFO, "Command not implemented");
            }
        }

        class LevelCommand : ChatCommand
        {
            public LevelCommand(string command, string syntax) : base(command, syntax) { }

            public override unsafe void Execute(ENetPeer* peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                var split = arguments.ToLower().Split(' ');
                float lvl;
                if (split.Length < 2)
                {
                    SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                    this.ShowSyntax();
                }
                else if (float.TryParse(split[1], out lvl))
                {
                    if (lvl < 1 || lvl > 18)
                        return;
                    game.getPeerInfo(peer).getChampion().getStats().setExp(game.getMap().getExperienceToLevelUp()[(int)lvl - 1]);
                    //game.peerInfo(peer).getChampion().getStats().setLevel(lvl);
                }
            }
        }

        class ManaCommand : ChatCommand
        {
            public ManaCommand(string command, string syntax) : base(command, syntax) { }

            public override unsafe void Execute(ENetPeer* peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                var split = arguments.ToLower().Split(' ');
                float mp;
                if (split.Length < 2)
                {
                    SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                    this.ShowSyntax();
                }
                else if (float.TryParse(split[1], out mp))
                {
                    game.getPeerInfo(peer).getChampion().getStats().setCurrentMana(mp);
                    game.getPeerInfo(peer).getChampion().getStats().setMaxMana(mp);
                }
            }
        }

        class MobsCommand : ChatCommand
        {
            public MobsCommand(string command, string syntax) : base(command, syntax) { }

            public override unsafe void Execute(ENetPeer* peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                var split = arguments.ToLower().Split(' ');
                if (split.Length < 2)
                {
                    SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                    this.ShowSyntax();
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
        }

        class ModelCommand : ChatCommand
        {
            public ModelCommand(string command, string syntax) : base(command, syntax) { }

            public override unsafe void Execute(ENetPeer* peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                var split = arguments.ToLower().Split(' ');
                if (split.Length >= 2)
                    game.getPeerInfo(peer).getChampion().setModel(split[1]);
                else
                {
                    SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                    this.ShowSyntax();
                }
            }
        }

        class PacketCommand : ChatCommand
        {
            public PacketCommand(string command, string syntax) : base(command, syntax) { }

            public override unsafe void Execute(ENetPeer* peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                try
                {
                    var s = arguments.Split(' ');
                    if (s.Length < 2)
                    {
                        SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                        this.ShowSyntax();
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

                    PacketHandlerManager.getInstace().sendPacket(peer, bytes.ToArray(), Channel.CHL_S2C);
                }
                catch { }
            }
        }

        class SetCommand : ChatCommand
        {
            public SetCommand(string command, string syntax) : base(command, syntax) { }

            public override unsafe void Execute(ENetPeer* peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                var split = arguments.ToLower().Split(' ');
                if (split.Length < 4)
                {
                    SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                    this.ShowSyntax();
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
        }

        class SizeCommand : ChatCommand
        {
            public SizeCommand(string command, string syntax) : base(command, syntax) { }

            public override unsafe void Execute(ENetPeer* peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                var split = arguments.ToLower().Split(' ');
                float size;
                if (split.Length < 2)
                {
                    SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                    this.ShowSyntax();
                    return;
                }
                else if (float.TryParse(split[1], out size))
                    game.getPeerInfo(peer).getChampion().getStats().setSize(size);
            }
        }

        class SkillpointsCommand : ChatCommand
        {
            public SkillpointsCommand(string command, string syntax) : base(command, syntax) { }

            public override unsafe void Execute(ENetPeer* peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                game.getPeerInfo(peer).getChampion().setSkillPoints(17);
                var skillUpResponse = new SkillUpPacket(game.getPeerInfo(peer).getChampion().getNetId(), 0, 0, 17);
                PacketHandlerManager.getInstace().sendPacket(peer, skillUpResponse, Channel.CHL_GAMEPLAY);
            }
        }

        class SpawnCommand : ChatCommand
        {
            public SpawnCommand(string command, string syntax) : base(command, syntax) { }

            public override unsafe void Execute(ENetPeer* peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                Logger.LogCoreInfo(".spawn command not implemented");
                SendDebugMsgFormatted(DebugMsgType.INFO, "Command not implemented");
            }
        }

        class SpeedCommand : ChatCommand
        {
            public SpeedCommand(string command, string syntax) : base(command, syntax) { }

            public override unsafe void Execute(ENetPeer* peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                var split = arguments.ToLower().Split(' ');
                float speed;
                if (split.Length < 2)
                {
                    SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                    this.ShowSyntax();
                }
                if (float.TryParse(split[1], out speed))
                    game.getPeerInfo(peer).getChampion().getStats().setMovementSpeed(speed);
                else
                    SendDebugMsgFormatted(DebugMsgType.ERROR, "Incorrect parameter");
            }
        }

        class TpCommand : ChatCommand
        {
            public TpCommand(string command, string syntax) : base(command, syntax) { }

            public override unsafe void Execute(ENetPeer* peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                var split = arguments.ToLower().Split(' ');
                float x, y;
                if (split.Length < 3)
                {
                    SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                    this.ShowSyntax();
                }
                if (float.TryParse(split[1], out x))
                    if (float.TryParse(split[2], out y))
                        PacketNotifier.notifyTeleport(game.getPeerInfo(peer).getChampion(), x, y);
            }
        }

        class XpCommand : ChatCommand
        {
            public XpCommand(string command, string syntax) : base(command, syntax) { }

            public override unsafe void Execute(ENetPeer* peer, Game game, bool hasReceivedArguments, string arguments = "")
            {
                var split = arguments.ToLower().Split(' ');
                float xp;
                if (split.Length < 2)
                {
                    SendDebugMsgFormatted(DebugMsgType.SYNTAXERROR);
                    this.ShowSyntax();
                }
                if (float.TryParse(split[1], out xp))
                    game.getPeerInfo(peer).getChampion().getStats().setExp(xp);
            }
        }
        #endregion

        #region SendDebugMsgFormatted:
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
        #endregion

        public unsafe bool HandlePacket(ENetPeer* peer, byte[] data, Game game)
        {
            #region Add commands here:
            var adCmd = new AdCommand(".ad", ".ad bonusAd");
            var apCmd = new ApCommand(".ap", ".ap bonusAp");
            var chCmd = new ChCommand(".ch", ".ch championName");
            var coordsCmd = new CoordsCommand(".coords", "");
            var goldCmd = new GoldCommand(".gold", ".gold goldAmount");
            var healthCmd = new HealthCommand(".health", ".health maxHealth");
            var helpCmd = new HelpCommand(".help", "");
            var inhibCmd = new InhibCommand(".inhib", "");
            var junglespawnCmd = new JunglespawnCommand(".junglespawn", "");
            var levelCmd = new LevelCommand(".level", ".level level");
            var manaCmd = new ManaCommand(".mana", ".mana maxMana");
            var mobsCmd = new MobsCommand(".mobs", ".mobs teamNumber");
            var modelCmd = new ModelCommand(".model", ".model modelName");
            var packetCmd = new PacketCommand(".packet", "No idea, too lazy to read the code");
            var setCmd = new SetCommand(".set", ".set masterMask fieldMask");
            var sizeCmd = new SizeCommand(".size", ".size size");
            var skillpointsCmd = new SkillpointsCommand(".skillpoints", "");
            var spawnCmd = new SpawnCommand(".spawn", "");
            var speedCmd = new SpeedCommand(".speed", ".speed speed");
            var tpCmd = new TpCommand(".tp", ".tp x y");
            var xpCmd = new XpCommand(".xp", ".xp xp");
            #endregion

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
            // Execute commands
            if (message.msg.StartsWith("."))
            {
                split = message.msg.ToLower().Split(' ');

                if (ChatCommandsDictionary.ContainsKey(split[0]))
                {
                    if (ChatCommandsDictionary[split[0]].IsHidden) // This could also check if the user has access to hidden commands
                    {
                        SendDebugMsgFormatted(DebugMsgType.ERROR, "<font color =\"#E175FF\"><b>" + split[0] + "</b><font color =\"#AFBF00\"> is not a valid command.");
                        SendDebugMsgFormatted(DebugMsgType.INFO, "Type <font color =\"#E175FF\"><b>.help</b><font color =\"#AFBF00\"> for a list of available commands");
                        return true;
                    }
                    else if (ChatCommandsDictionary[split[0]].IsDisabled)
                    {
                        SendDebugMsgFormatted(DebugMsgType.ERROR, "<font color =\"#E175FF\"><b>" + split[0] + "</b><font color =\"#AFBF00\"> is disabled.");
                        return true;
                    }
                    ChatCommandsDictionary[split[0]].Execute(peer, game, true, message.msg);
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
