using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Chatbox;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatboxManager;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleChatBoxMessage : IPacketHandler
    {
        private Game _game = Program.ResolveDependency<Game>();
        private ChatboxManager _chatboxManager = Program.ResolveDependency<ChatboxManager>();
        private PlayerManager _playerManager = Program.ResolveDependency<PlayerManager>();

        public bool HandlePacket(Peer peer, byte[] data)
        {
            var message = new ChatMessage(data);
            var split = message.msg.Split(' ');
            if (split.Length > 1)
            {
                int x, y = 0;
                if (int.TryParse(split[0], out x))
                {
                    if (int.TryParse(split[1], out y))
                    {
                        var response = new AttentionPingAns(
                            _playerManager.GetPeerInfo(peer),
                            new AttentionPing { x = x, y = y, targetNetId = 0, type = 0 }
                        );
                        _game.PacketHandlerManager.broadcastPacketTeam(
                            _playerManager.GetPeerInfo(peer).Team, response, Channel.CHL_S2C
                        );
                    }
                }
            }

            #region Commands
            // Execute commands
            var CommandStarterCharacter = _chatboxManager.CommandStarterCharacter;
            if (message.msg.StartsWith(CommandStarterCharacter))
            {
                message.msg = message.msg.Remove(0, 1);
                split = message.msg.ToLower().Split(' ');

                ChatCommand command = _chatboxManager.GetCommand(split[0]);
                if (command != null)
                {
                    command.Execute(peer, true, message.msg);
                    return true;
                }
                else
                {
                    _chatboxManager.SendDebugMsgFormatted(DebugMsgType.ERROR, "<font color =\"#E175FF\"><b>" + _chatboxManager.CommandStarterCharacter + split[0] + "</b><font color =\"#AFBF00\"> is not a valid command.");
                    _chatboxManager.SendDebugMsgFormatted(DebugMsgType.INFO, "Type <font color =\"#E175FF\"><b>" + _chatboxManager.CommandStarterCharacter + "help</b><font color =\"#AFBF00\"> for a list of available commands");
                    return true;
                }
            }
            #endregion
            switch (message.type)
            {
                case ChatType.CHAT_ALL:
                    return _game.PacketHandlerManager.broadcastPacket(data, Channel.CHL_COMMUNICATION);
                case ChatType.CHAT_TEAM:
                    return _game.PacketHandlerManager.broadcastPacketTeam(
                        _playerManager.GetPeerInfo(peer).Team, data, Channel.CHL_COMMUNICATION
                    );
                default:
                    //Logging.errorLine("Unknown ChatMessageType");
                    return _game.PacketHandlerManager.sendPacket(peer, data, Channel.CHL_COMMUNICATION);
            }
        }
    }
}
