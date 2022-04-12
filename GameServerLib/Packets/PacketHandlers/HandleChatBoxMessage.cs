using GameServerCore.Packets.PacketDefinitions.Requests;
using GameServerCore;
using GameServerCore.Enums;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Handlers;
using LeagueSandbox.GameServer.Chatbox;
using LeagueSandbox.GameServer.Logging;
using log4net;
using System.Numerics;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleChatBoxMessage : PacketHandlerBase<ChatMessageRequest>
    {
        private readonly Game _game;
        private readonly ChatCommandManager _chatCommandManager;
        private readonly IPlayerManager _playerManager;
        private readonly ILog _logger;

        public HandleChatBoxMessage(Game game)
        {
            _game = game;
            _chatCommandManager = game.ChatCommandManager;
            _playerManager = game.PlayerManager;
            _logger = LoggerProvider.GetLogger();
        }

        public override bool HandlePacket(int userId, ChatMessageRequest req)
        {
            var split = req.Message.Split(' ');
            if (split.Length > 1)
            {
                if (int.TryParse(split[0], out var x))
                {
                    if (int.TryParse(split[1], out var y))
                    {
                        var client = _playerManager.GetPeerInfo(userId);
                        _game.PacketNotifier.NotifyS2C_MapPing(new Vector2(x, y), Pings.PING_DEFAULT, client: client);
                    }
                }
            }

            // Execute commands
            var commandStarterCharacter = _chatCommandManager.CommandStarterCharacter;
            if (req.Message.StartsWith(commandStarterCharacter))
            {
                var msg = req.Message.Remove(0, 1);
                split = msg.ToLower().Split(' ');

                var command = _chatCommandManager.GetCommand(split[0]);
                if (command != null)
                {
                    try
                    {
                        command.Execute(userId, true, msg);
                    }
                    catch
                    {
                        _logger.Warn(command + " sent an exception.");
                        _game.PacketNotifier.NotifyS2C_SystemMessage(userId, "Something went wrong...Did you wrote the command well ? ");
                    }
                    return true;
                }

                _chatCommandManager.SendDebugMsgFormatted(DebugMsgType.ERROR, "<font color =\"#E175FF\"><b>"
                                                                              + _chatCommandManager.CommandStarterCharacter + split[0] + "</b><font color =\"#AFBF00\"> " +
                                                                              "is not a valid command.");
                _chatCommandManager.SendDebugMsgFormatted(DebugMsgType.INFO, "Type <font color =\"#E175FF\"><b>"
                                                                             + _chatCommandManager.CommandStarterCharacter + "help</b><font color =\"#AFBF00\"> " +
                                                                             "for a list of available commands");
                return true;
            }

            var debugMessage =
                $"{_playerManager.GetPeerInfo(userId).Name} ({_playerManager.GetPeerInfo(userId).Champion.Model}): </font><font color=\"#FFFFFF\">{req.Message}";
            var teamChatColor = "<font color=\"#00FF00\">";
            var enemyChatColor = "<font color=\"#FF0000\">";
            var dmTeam = teamChatColor + "[All] " + debugMessage;
            var dmEnemy = enemyChatColor + "[All] " + debugMessage;
            var ownTeam = _playerManager.GetPeerInfo(userId).Team;
            var enemyTeam = CustomConvert.GetEnemyTeam(ownTeam);

            if (_game.Config.ChatCheatsEnabled)
            {
                _game.PacketNotifier.NotifyS2C_SystemMessage(ownTeam, dmTeam);
                _game.PacketNotifier.NotifyS2C_SystemMessage(enemyTeam, dmEnemy);
                return true;
            }

            var type = (ChatType)req.ChatType;
            switch (type)
            {
                case ChatType.All:
                    _game.PacketNotifier.NotifyS2C_SystemMessage(ownTeam, dmTeam);
                    _game.PacketNotifier.NotifyS2C_SystemMessage(enemyTeam, dmEnemy);
                    return true;
                case ChatType.Team:
                    _game.PacketNotifier.NotifyS2C_SystemMessage(ownTeam, dmTeam);
                    return true;
                default:
                    _logger.Error("Unknown ChatMessageType:" + req.ChatType.ToString());
                    return false;
            }
        }
    }
}
