using ENet;
using LeagueSandbox.GameServer.Logic.Chatbox;
using LeagueSandbox.GameServer.Logic.Logging;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
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
            var message = new ChatMessage(data);
            var split = message.Msg.Split(' ');
            if (split.Length > 1)
            {
                if (int.TryParse(split[0], out var x))
                {
                    if (int.TryParse(split[1], out var y))
                    {
                        var response = new AttentionPingResponse(_playerManager.GetPeerInfo(peer),
                            new AttentionPingRequest(x, y, 0, Pings.PING_DEFAULT)
                        );
                        _game.PacketHandlerManager.BroadcastPacketTeam(
                            _playerManager.GetPeerInfo(peer).Team, response, Channel.CHL_S2C
                        );
                    }
                }
            }

            // Execute commands
            var commandStarterCharacter = _chatCommandManager.CommandStarterCharacter;
            if (message.Msg.StartsWith(commandStarterCharacter))
            {
                message.Msg = message.Msg.Remove(0, 1);
                split = message.Msg.ToLower().Split(' ');

                var command = _chatCommandManager.GetCommand(split[0]);
                if (command != null)
                {
                    try
                    {
                        command.Execute(peer, true, message.Msg);
                    }
                    catch
                    {
                        _logger.Warning(command + " sent an exception.");
                        var dm = new DebugMessage("Something went wrong...Did you wrote the command well ? ");
                        _game.PacketHandlerManager.SendPacket(peer, dm, Channel.CHL_S2C);
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
                $"{_playerManager.GetPeerInfo(peer).Name} ({_playerManager.GetPeerInfo(peer).Champion.Model}): </font><font color=\"#FFFFFF\">{message.Msg}";
            var teamChatColor = "<font color=\"#00FF00\">";
            var enemyChatColor = "<font color=\"#FF0000\">";
            var dmTeam = new DebugMessage(teamChatColor + "[All] " + debugMessage);
            var dmEnemy = new DebugMessage(enemyChatColor + "[All] " + debugMessage);
            var ownTeam = _playerManager.GetPeerInfo(peer).Team;
            var enemyTeam = CustomConvert.GetEnemyTeam(ownTeam);

            if (_game.Config.ChatCheatsEnabled)
            {
                _game.PacketHandlerManager.BroadcastPacketTeam(ownTeam, dmTeam, Channel.CHL_S2C);
                _game.PacketHandlerManager.BroadcastPacketTeam(enemyTeam, dmEnemy, Channel.CHL_S2C);
                return true;
            }

            switch (message.Type)
            {
                case ChatType.CHAT_ALL:
                    _game.PacketHandlerManager.BroadcastPacketTeam(ownTeam, dmTeam, Channel.CHL_S2C);
                    _game.PacketHandlerManager.BroadcastPacketTeam(enemyTeam, dmEnemy, Channel.CHL_S2C);
                    return true;
                case ChatType.CHAT_TEAM:
                    dmTeam = new DebugMessage(teamChatColor + debugMessage);
                    _game.PacketHandlerManager.BroadcastPacketTeam(ownTeam, dmTeam, Channel.CHL_S2C);
                    return true;
                default:
                    //Logging.errorLine("Unknown ChatMessageType");
                    return _game.PacketHandlerManager.SendPacket(peer, data, Channel.CHL_COMMUNICATION);
            }
        }
    }
}
