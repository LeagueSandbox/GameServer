using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LeagueSandbox.GameServer.Chatbox
{
    public class ChatCommandManager
    {
        private readonly Game _game;

        public string CommandStarterCharacter = "!";

        private SortedDictionary<string, IChatCommand> _chatCommandsDictionary;

        // TODO: Refactor this method or maybe the packet notifier?
        public void SendDebugMsgFormatted(DebugMsgType type, string message = "")
        {
            var formattedText = new StringBuilder();
            var fontSize = 20; // Big fonts seem to make the chatbox buggy
                               // This may need to be removed.
            switch (type)
            {
                case DebugMsgType.ERROR: // Tag: [ERROR], Color: Red
                    formattedText.Append("<font size=\"" + fontSize + "\" color =\"#FF0000\"><b>[ERROR]</b><font color =\"#AFBF00\">: ");
                    formattedText.Append(message);
                    _game.PacketNotifier.NotifyDebugMessage(formattedText.ToString());
                    break;
                case DebugMsgType.INFO: // Tag: [INFO], Color: Green
                    formattedText.Append("<font size=\"" + fontSize + "\" color =\"#00D90E\"><b>[LS INFO]</b><font color =\"#AFBF00\">: ");
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

        public ChatCommandManager(Game game)
        {
            _game = game;
            _chatCommandsDictionary = new SortedDictionary<string, IChatCommand>();
        }

        public void LoadCommands()
        {
            //TODO: cyclic dependency
            if (!_game.Config.ChatCheatsEnabled)
            {
                return;
            }

            var loadFrom = new[] { ServerLibAssemblyDefiningType.Assembly };
            _chatCommandsDictionary = GetAllChatCommandHandlers(loadFrom, _game);
        }

        internal SortedDictionary<string, IChatCommand> GetAllChatCommandHandlers(Assembly[] loadFromArray, Game game)
        {
            var commands = new List<IChatCommand>();
            var args = new object[] {this, game};
            foreach (var loadFrom in loadFromArray)
            {
                commands.AddRange(loadFrom.GetTypes()
                    .Where(t => t.BaseType == typeof(ChatCommandBase))
                    .Select(t => (IChatCommand) Activator.CreateInstance(t, args)));
            }
            var commandsOutput = new SortedDictionary<string, IChatCommand>();

            foreach (var converter in commands)
            {
                commandsOutput.Add(converter.Command, converter);
            }

            return commandsOutput;
        }

        public bool AddCommand(IChatCommand command)
        {
            if (_chatCommandsDictionary.ContainsKey(command.Command))
            {
                return false;
            }

            _chatCommandsDictionary.Add(command.Command, command);
            return true;
        }

        public bool RemoveCommand(IChatCommand command)
        {
            if (!_chatCommandsDictionary.ContainsValue(command))
            {
                return false;
            }

            _chatCommandsDictionary.Remove(command.Command);
            return true;
        }

        public bool RemoveCommand(string commandString)
        {
            if (!_chatCommandsDictionary.ContainsKey(commandString))
            {
                return false;
            }

            _chatCommandsDictionary.Remove(commandString);
            return true;
        }

        public List<IChatCommand> GetCommands()
        {
            return _chatCommandsDictionary.Values.ToList();
        }

        public List<string> GetCommandsStrings()
        {
            return _chatCommandsDictionary.Keys.ToList();
        }

        public IChatCommand GetCommand(string commandString)
        {
            if (_chatCommandsDictionary.ContainsKey(commandString))
            {
                return _chatCommandsDictionary[commandString];
            }

            return null;
        }
    }
}
