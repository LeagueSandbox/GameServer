﻿using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Chatbox.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic.Handlers;

namespace LeagueSandbox.GameServer.Logic.Chatbox
{
    public class ChatCommandManager
    {
        private readonly IHandlersProvider _handlersProvider;

        public string CommandStarterCharacter = ".";

        private SortedDictionary<string, IChatCommand> _chatCommandsDictionary;

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

        public ChatCommandManager(IHandlersProvider handlersProvider)
        {
            _handlersProvider = handlersProvider;
            _chatCommandsDictionary = new SortedDictionary<string, IChatCommand>();
        }

        public void LoadCommands()
        {
            //TODO: cyclic dependency
            var _game = Program.ResolveDependency<Game>();
            if (!_game.Config.ChatCheatsEnabled)
                return;

            var loadFrom = new[] { ServerLibAssemblyDefiningType.Assembly };
            _chatCommandsDictionary = _handlersProvider.GetAllChatCommandHandlers(loadFrom);
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
