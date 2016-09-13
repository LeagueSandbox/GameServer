using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Players;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatboxManager;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    class CoordsCommand : ChatCommand
    {
        public CoordsCommand(string command, string syntax, ChatboxManager owner) : base(command, syntax, owner) { }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            Logger logger = Program.ResolveDependency<Logger>();
            PlayerManager playerManager = Program.ResolveDependency<PlayerManager>();

            var champion = playerManager.GetPeerInfo(peer).Champion;

            logger.LogCoreInfo(string.Format(
                "At {0}; {1}",
                champion.X,
                champion.Y
            ));

            _owner.SendDebugMsgFormatted(
                DebugMsgType.NORMAL,
                string.Format(
                    "At Coords - X: {0} Y: {1} Z: {2}",
                    champion.X,
                    champion.Y,
                    champion.GetZ()
                )
            );
        }
    }
}
