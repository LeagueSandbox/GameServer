using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LeagueSandbox.GameServer.Logic.Chatbox
{
    public static class ChatCommandManager
    {
        //private readonly IHandlersProvider _handlersProvider;

        public static string CommandStarterCharacter = ".";

        private static SortedDictionary<string, IChatCommand> _chatCommandsDictionary = new SortedDictionary<string, IChatCommand>();

        // TODO: Refactor this method or maybe the packet notifier?
        public static void SendDebugMsgFormatted(DebugMsgType type, string message = "")
        {
            //var game = Program.ResolveDependency<Game>();
            var formattedText = new StringBuilder();
            var fontSize = 20; // Big fonts seem to make the chatbox buggy
                               // This may need to be removed.
            switch (type)
            {
                case DebugMsgType.ERROR: // Tag: [ERROR], Color: Red
                    formattedText.Append("<font size=\"" + fontSize + "\" color =\"#FF0000\"><b>[ERROR]</b><font color =\"#AFBF00\">: ");
                    formattedText.Append(message);
                    Game.PacketNotifier.NotifyDebugMessage(formattedText.ToString());
                    break;
                case DebugMsgType.INFO: // Tag: [INFO], Color: Green
                    formattedText.Append("<font size=\"" + fontSize + "\" color =\"#00D90E\"><b>[INFO]</b><font color =\"#AFBF00\">: ");
                    formattedText.Append(message);
                    Game.PacketNotifier.NotifyDebugMessage(formattedText.ToString());
                    break;
                case DebugMsgType.SYNTAX: // Tag: [SYNTAX], Color: Blue
                    formattedText.Append("<font size=\"" + fontSize + "\" color =\"#006EFF\"><b>[SYNTAX]</b><font color =\"#AFBF00\">: ");
                    formattedText.Append(message);
                    Game.PacketNotifier.NotifyDebugMessage(formattedText.ToString());
                    break;
                case DebugMsgType.SYNTAXERROR: // Tag: [ERROR], Color: Red
                    formattedText.Append("<font size=\"" + fontSize + "\" color =\"#FF0000\"><b>[ERROR]</b><font color =\"#AFBF00\">: ");
                    formattedText.Append("Incorrect command syntax");
                    Game.PacketNotifier.NotifyDebugMessage(formattedText.ToString());
                    break;
                case DebugMsgType.NORMAL: // No tag, no format
                    Game.PacketNotifier.NotifyDebugMessage(message);
                    break;
            }
        }

        public static void LoadCommands()
        {
            //TODO: cyclic dependency
            //var game = Program.ResolveDependency<Game>();
            if (!Game.Config.ChatCheatsEnabled)
            {
                return;
            }
            
            //ChatCommandBase
            _chatCommandsDictionary = GetInstances(ServerLibAssemblyDefiningType.Assembly);
            //Console.WriteLine("Break");
        }

        internal static SortedDictionary<string, IChatCommand> GetInstances(Assembly loadFrom)
        {
            var all = (from t in loadFrom.GetTypes()
                where t.BaseType == (typeof(ChatCommandBase)) && t.GetConstructor(Type.EmptyTypes) != null
                select (IChatCommand)Activator.CreateInstance(t)).ToList();

            var commands = new SortedDictionary<string, IChatCommand>();
            foreach (var converter in all)
            {
                commands.Add(converter.Command, converter);
            }

            return commands;
        }

        public static bool AddCommand(IChatCommand command)
        {
            if (_chatCommandsDictionary.ContainsKey(command.Command))
            {
                return false;
            }

            _chatCommandsDictionary.Add(command.Command, command);
            return true;
        }

        public static bool RemoveCommand(IChatCommand command)
        {
            if (!_chatCommandsDictionary.ContainsValue(command))
            {
                return false;
            }

            _chatCommandsDictionary.Remove(command.Command);
            return true;
        }

        public static bool RemoveCommand(string commandString)
        {
            if (!_chatCommandsDictionary.ContainsKey(commandString))
            {
                return false;
            }

            _chatCommandsDictionary.Remove(commandString);
            return true;
        }

        public static List<IChatCommand> GetCommands()
        {
            return _chatCommandsDictionary.Values.ToList();
        }

        public static List<string> GetCommandsStrings()
        {
            return _chatCommandsDictionary.Keys.ToList();
        }

        public static IChatCommand GetCommand(string commandString)
        {
            if (_chatCommandsDictionary.ContainsKey(commandString))
            {
                return _chatCommandsDictionary[commandString];
            }

            return null;
        }
    }
}
