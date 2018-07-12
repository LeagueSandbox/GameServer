using ENet;
using LeagueSandbox.GameServer.Logic.Chatbox;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleChatBoxMessage : PacketHandlerBase
    {

        public override PacketCmd PacketType => PacketCmd.PKT_CHAT_BOX_MESSAGE;
        public override Channel PacketChannel => Channel.CHL_COMMUNICATION;

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
                        var response = new AttentionPingResponse(
                            PlayerManager.GetPeerInfo(peer),
                            new AttentionPingRequest(x, y, 0, Pings.PING_DEFAULT)
                        );
                        Game.PacketHandlerManager.BroadcastPacketTeam(
                            PlayerManager.GetPeerInfo(peer).Team, response, Channel.CHL_S2_C
                        );
                    }
                }
            }

            // Execute commands
            var commandStarterCharacter = ChatCommandManager.CommandStarterCharacter;
            if (message.Msg.StartsWith(commandStarterCharacter))
            {
                message.Msg = message.Msg.Remove(0, 1);
                split = message.Msg.ToLower().Split(' ');

                var command = ChatCommandManager.GetCommand(split[0]);
                if (command != null)
                {
                    try
                    {
                        command.Execute(peer, true, message.Msg);
                    }
                    catch
                    {
                        Logger.LogCoreWarning(command + " sent an exception.");
                        var dm = new DebugMessage("Something went wrong...Did you wrote the command well ? ");
                        Game.PacketHandlerManager.SendPacket(peer, dm, Channel.CHL_S2_C);
                    }
                    return true;
                }

                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.ERROR, "<font color =\"#E175FF\"><b>"
                                                                              + ChatCommandManager.CommandStarterCharacter + split[0] + "</b><font color =\"#AFBF00\"> " +
                                                                              "is not a valid command.");
                ChatCommandManager.SendDebugMsgFormatted(DebugMsgType.INFO, "Type <font color =\"#E175FF\"><b>"
                                                                             + ChatCommandManager.CommandStarterCharacter + "help</b><font color =\"#AFBF00\"> " +
                                                                             "for a list of available commands");
                return true;
            }

            var debugMessage =
                $"{PlayerManager.GetPeerInfo(peer).Name} ({PlayerManager.GetPeerInfo(peer).Champion.Model}): </font><font color=\"#FFFFFF\">{message.Msg}";
            var teamChatColor = "<font color=\"#00FF00\">";
            var enemyChatColor = "<font color=\"#FF0000\">";
            var dmTeam = new DebugMessage(teamChatColor + "[All] " + debugMessage);
            var dmEnemy = new DebugMessage(enemyChatColor + "[All] " + debugMessage);
            var ownTeam = PlayerManager.GetPeerInfo(peer).Team;
            var enemyTeam = CustomConvert.GetEnemyTeam(ownTeam);

            if (Game.Config.ChatCheatsEnabled)
            {
                Game.PacketHandlerManager.BroadcastPacketTeam(ownTeam, dmTeam, Channel.CHL_S2_C);
                Game.PacketHandlerManager.BroadcastPacketTeam(enemyTeam, dmEnemy, Channel.CHL_S2_C);
                return true;
            }

            switch (message.Type)
            {
                case ChatType.CHAT_ALL:
                    Game.PacketHandlerManager.BroadcastPacketTeam(ownTeam, dmTeam, Channel.CHL_S2_C);
                    Game.PacketHandlerManager.BroadcastPacketTeam(enemyTeam, dmEnemy, Channel.CHL_S2_C);
                    return true;
                case ChatType.CHAT_TEAM:
                    dmTeam = new DebugMessage(teamChatColor + debugMessage);
                    Game.PacketHandlerManager.BroadcastPacketTeam(ownTeam, dmTeam, Channel.CHL_S2_C);
                    return true;
                default:
                    //Logging.errorLine("Unknown ChatMessageType");
                    return Game.PacketHandlerManager.SendPacket(peer, data, Channel.CHL_COMMUNICATION);
            }
        }
    }
}
