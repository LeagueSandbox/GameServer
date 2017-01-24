using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Chatbox.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Chatbox
{
    public class ChatCommandManager
    {
        public string CommandStarterCharacter = ".";

        private SortedDictionary<string, ChatCommand> _chatCommandsDictionary = new SortedDictionary<string, ChatCommand>
        {
            /*
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
            {".xp",  new XpCommand(".xp", ".xp xp")}*/
        };

        public enum DebugMsgType
        {
            ERROR,
            INFO,
            SYNTAX,
            SYNTAXERROR,
            NORMAL
        }

        // TODO: Refactor this method or maybe the packet notifier?
        public void SendDebugMsgFormatted(DebugMsgType type, string message = "")
        {
            var _game = Program.ResolveDependency<Game>();
            var formattedText = new StringBuilder();
            int fontSize = 20; // Big fonts seem to make the chatbox buggy
                               // This may need to be removed.
            switch (type)
            {
                case DebugMsgType.ERROR: // Tag: [ERROR], Color: Red
                    formattedText.Append("<font size=\"" + fontSize + "\" color =\"#FF0000\"><b>[ERROR]</b><font color =\"#AFBF00\">: ");
                    formattedText.Append(message);
                    _game.PacketNotifier.NotifyDebugMessage(formattedText.ToString());
                    break;
                case DebugMsgType.INFO: // Tag: [INFO], Color: Green
                    formattedText.Append("<font size=\"" + fontSize + "\" color =\"#00D90E\"><b>[INFO]</b><font color =\"#AFBF00\">: ");
                    formattedText.Append(message);
                    _game.PacketNotifier.NotifyDebugMessage(formattedText.ToString());
                    break;
                case DebugMsgType.SYNTAX: // Tag: [SYNTAX], Color: Blue
                    formattedText.Append("<font size=\"" + fontSize + "\" color =\"#006EFF\"><b>[SYNTAX]</b><font color =\"#AFBF00\">: ");
                    formattedText.Append(message);
                    _game.PacketNotifier.NotifyDebugMessage(formattedText.ToString());
                    break;
                case DebugMsgType.SYNTAXERROR: // Tag: [ERROR], Color: Red
                    formattedText.Append("<font size=\"" + fontSize + "\" color =\"#FF0000\"><b>[ERROR]</b><font color =\"#AFBF00\">: ");
                    formattedText.Append("Incorrect command syntax");
                    _game.PacketNotifier.NotifyDebugMessage(formattedText.ToString());
                    break;
                case DebugMsgType.NORMAL: // No tag, no format
                    _game.PacketNotifier.NotifyDebugMessage(message);
                    break;
            }
        }

        public ChatCommandManager()
        {
            AddCommand(new HelpCommand("help", "", this));
        }

        public void LoadCommands()
        {
            var _game = Program.ResolveDependency<Game>();
            if (!_game.Config.ChatCheatsEnabled)
            {
                return;
            }

            AddCommand(new AdCommand("ad", "ad bonusAd", this));
            AddCommand(new ApCommand("ap", "ap bonusAp", this));
            AddCommand(new ChangeTeamCommand("changeteam", "changeteam teamNumber", this));
            AddCommand(new ChCommand("ch", "ch championName", this));
            AddCommand(new CoordsCommand("coords", "", this));
            AddCommand(new GoldCommand("gold", "gold goldAmount", this));
            AddCommand(new HealthCommand("health", "health maxHealth", this));
            AddCommand(new InhibCommand("inhib", "", this));
            AddCommand(new JunglespawnCommand("junglespawn", "", this));
            AddCommand(new KillCommand("kill", "kill minions", this));
            AddCommand(new LevelCommand("level", "level level", this));
            AddCommand(new ManaCommand("mana", "mana maxMana", this));
            AddCommand(new MobsCommand("mobs", "mobs teamNumber", this));
            AddCommand(new ModelCommand("model", "model modelName", this));
            AddCommand(new PacketCommand("packet", "packet XX XX XX...", this));
            AddCommand(new RainbowCommand("rainbow", "rainbow alpha speed", this));
            AddCommand(new ReloadLuaCommand("reloadlua", "", this));
            AddCommand(new ReviveCommand("revive", "", this));
            AddCommand(new SetCommand("set", "set masterMask fieldMask", this));
            AddCommand(new SizeCommand("size", "size size", this));
            AddCommand(new SkillpointsCommand("skillpoints", "", this));
            AddCommand(new SpawnCommand("spawn", "spawn minions", this));
            AddCommand(new SpawnStateCommand("spawnstate", "spawnstate 0 (disable) / 1 (enable)", this));
            AddCommand(new SpeedCommand("speed", "speed speed", this));
            AddCommand(new TpCommand("tp", "tp x y", this));
            AddCommand(new XpCommand("xp", "xp xp", this));
        }

        public bool AddCommand(ChatCommand command)
        {
            if (_chatCommandsDictionary.ContainsKey(command.Command))
            {
                return false;
            }

            _chatCommandsDictionary.Add(command.Command, command);
            return true;
        }

        public bool RemoveCommand(ChatCommand command)
        {
            if (_chatCommandsDictionary.ContainsValue(command))
            {
                _chatCommandsDictionary.Remove(command.Command);
                return true;
            }

            return false;
        }

        public bool RemoveCommand(string commandString)
        {
            if (_chatCommandsDictionary.ContainsKey(commandString))
            {
                _chatCommandsDictionary.Remove(commandString);
                return true;
            }

            return false;
        }

        public List<ChatCommand> GetCommands()
        {
            return _chatCommandsDictionary.Values.ToList();
        }

        public List<string> GetCommandsStrings()
        {
            return _chatCommandsDictionary.Keys.ToList();
        }

        public ChatCommand GetCommand(string commandString)
        {
            if (_chatCommandsDictionary.ContainsKey(commandString))
            {
                return _chatCommandsDictionary[commandString];
            }

            return null;
        }
    }
}
