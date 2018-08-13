using ENet;
using GameServerCore;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Interfaces;
using LeagueSandbox.GameServer.Logic.Chatbox;
using LeagueSandbox.GameServer.Logic.Logging;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleChatBoxMessage : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly ChatCommandManager _chatCommandManager;
        private readonly PlayerManager _playerManager;
        private readonly ILogger _logger;

        public override PacketCmd PacketType => PacketCmd.PKT_CHAT_BOX_MESSAGE;
        public override Channel PacketChannel => Channel.CHL_COMMUNICATION;

        public HandleChatBoxMessage(Game game)
        {
            _game = game;
            _chatCommandManager = game.ChatCommandManager;
            _playerManager = game.PlayerManager;
            _logger = LoggerProvider.GetLogger();
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var request = _game.PacketReader.ReadChatMessageRequest(data);
            var split = request.Message.Split(' ');
            if (split.Length > 1)
            {
                if (int.TryParse(split[0], out var x))
                {
                    if (int.TryParse(split[1], out var y))
                    {
                        var client = _playerManager.GetPeerInfo(peer);
                         _game.PacketNotifier.NotifyPing(client, x, y, 0, Pings.PING_DEFAULT);
                    }
                }
            }

            // Execute commands
            var commandStarterCharacter = _chatCommandManager.CommandStarterCharacter;
            if (request.Message.StartsWith(commandStarterCharacter))
            {
                var msg = request.Message.Remove(0, 1);
                split = msg.ToLower().Split(' ');

                var command = _chatCommandManager.GetCommand(split[0]);
                if (command != null)
                {
                    try
                    {
                        command.Execute(peer, true, msg);
                    }
                    catch
                    {
                        _logger.Warning(command + " sent an exception.");
                         _game.PacketNotifier.NotifyDebugMessage(peer, "Something went wrong...Did you wrote the command well ? ");
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
                $"{_playerManager.GetPeerInfo(peer).Name} ({_playerManager.GetPeerInfo(peer).Champion.Model}): </font><font color=\"#FFFFFF\">{request.Message}";
            var teamChatColor = "<font color=\"#00FF00\">";
            var enemyChatColor = "<font color=\"#FF0000\">";
            var dmTeam = teamChatColor + "[All] " + debugMessage;
            var dmEnemy = enemyChatColor + "[All] " + debugMessage;
            var ownTeam = _playerManager.GetPeerInfo(peer).Team;
            var enemyTeam = CustomConvert.GetEnemyTeam(ownTeam);

            if (_game.Config.ChatCheatsEnabled)
            {
                 _game.PacketNotifier.NotifyDebugMessage(ownTeam, dmTeam);
                 _game.PacketNotifier.NotifyDebugMessage(enemyTeam, dmEnemy);
                return true;
            }

            switch (request.Type)
            {
                case ChatType.CHAT_ALL:
                     _game.PacketNotifier.NotifyDebugMessage(ownTeam, dmTeam);
                     _game.PacketNotifier.NotifyDebugMessage(enemyTeam, dmEnemy);
                    return true;
                case ChatType.CHAT_TEAM:
                     _game.PacketNotifier.NotifyDebugMessage(ownTeam, dmTeam);
                    return true;
                default:
                    //Logging.errorLine("Unknown ChatMessageType");
                    return _game.PacketHandlerManager.SendPacket(peer, data, Channel.CHL_COMMUNICATION);
            }
        }
    }
}
