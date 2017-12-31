using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Chatbox;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleChatBoxMessage : PacketHandlerBase<ChatMessage>
    {
        private readonly Game _game;
        private readonly ChatCommandManager _chatCommandManager;
        private readonly PlayerManager _playerManager;
        private readonly Logger _logger;

        public override PacketCmd PacketType => PacketCmd.PKT_ChatBoxMessage;
        public override Channel PacketChannel => Channel.CHL_COMMUNICATION;

        public HandleChatBoxMessage(Game game, ChatCommandManager chatCommandManager, PlayerManager playerManager,
            Logger logger)
        {
            _game = game;
            _chatCommandManager = chatCommandManager;
            _playerManager = playerManager;
            _logger = logger;
        }

        public override bool HandlePacketInternal(Peer peer, ChatMessage data)
        {
            var split = data.Msg.Split(' ');
            TryParsePingRequest(peer, split);

            #region Commands
            // Execute commands
            var commandStarterCharacter = _chatCommandManager.CommandStarterCharacter;
            if (data.Msg.StartsWith(commandStarterCharacter))
            {
                return HandleChatCommand(peer, data, split);
            }
            #endregion

            var debugMessage = string.Format("{0} ({1}): </font><font color=\"#FFFFFF\">{2}",
                _playerManager.GetPeerInfo(peer).Name,
                _playerManager.GetPeerInfo(peer).Champion.Model,
                data.Msg);
            var teamChatColor = "<font color=\"#00FF00\">";
            var enemyChatColor = "<font color=\"#FF0000\">";
            var dmTeam = new DebugMessage(teamChatColor + "[All] " + debugMessage);
            var dmEnemy = new DebugMessage(enemyChatColor + "[All] " + debugMessage);
            var ownTeam = _playerManager.GetPeerInfo(peer).Team;
            var enemyTeam = CustomConvert.GetEnemyTeam(ownTeam);

            if (_game.Config.ChatCheatsEnabled)
            {
                _game.PacketHandlerManager.broadcastPacketTeam(ownTeam, dmTeam, Channel.CHL_S2C);
                _game.PacketHandlerManager.broadcastPacketTeam(enemyTeam, dmEnemy, Channel.CHL_S2C);
                return true;
            }

            switch (data.Type)
            {
                case ChatType.CHAT_ALL:
                    _game.PacketHandlerManager.broadcastPacketTeam(ownTeam, dmTeam, Channel.CHL_S2C);
                    _game.PacketHandlerManager.broadcastPacketTeam(enemyTeam, dmEnemy, Channel.CHL_S2C);
                    return true;
                case ChatType.CHAT_TEAM:
                    dmTeam = new DebugMessage(teamChatColor + debugMessage);
                    _game.PacketHandlerManager.broadcastPacketTeam(ownTeam, dmTeam, Channel.CHL_S2C);
                    return true;
                default:
                    //Logging.errorLine("Unknown ChatMessageType");
                    return _game.PacketHandlerManager.sendPacket(peer, data.OriginalData, Channel.CHL_COMMUNICATION);
            }
        }

        private bool HandleChatCommand(Peer peer, ChatMessage data, string[] split)
        {
            var commandString = data.Msg.Remove(0, 1);
            split = commandString.ToLower().Split(' ');

            var command = _chatCommandManager.GetCommand(split[0]);
            if (command != null)
            {
                try
                {
                    command.Execute(peer, true, commandString);
                }
                catch
                {
                    _logger.LogCoreWarning(command + " sent an exception.");
                    var dm = new DebugMessage("Something went wrong...Did you wrote the command well ? ");
                    _game.PacketHandlerManager.sendPacket(peer, dm, Channel.CHL_S2C);
                }
                return true;
            }
            else
            {
                _chatCommandManager.SendDebugMsgFormatted(DebugMsgType.ERROR, "<font color =\"#E175FF\"><b>"
                    + _chatCommandManager.CommandStarterCharacter + split[0] + "</b><font color =\"#AFBF00\"> " +
                                                                              "is not a valid command.");
                _chatCommandManager.SendDebugMsgFormatted(DebugMsgType.INFO, "Type <font color =\"#E175FF\"><b>"
                    + _chatCommandManager.CommandStarterCharacter + "help</b><font color =\"#AFBF00\"> " +
                                                                             "for a list of available commands");
                return true;
            }
        }

        private void TryParsePingRequest(Peer peer, string[] split)
        {
            if (split.Length < 2)
                return;

            int x, y;
            if (!int.TryParse(split[0], out x))
                return;
            if (!int.TryParse(split[1], out y))
                return;

            var response = new AttentionPingResponse
            (
                _playerManager.GetPeerInfo(peer),
                x,
                y,
                0,
                Pings.Ping_Default
            );
            _game.PacketHandlerManager.broadcastPacketTeam(
                _playerManager.GetPeerInfo(peer).Team, response, Channel.CHL_S2C);
        }
    }
}
