﻿using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Players;
using System;
using System.Numerics;
using static LeagueSandbox.GameServer.Logic.Chatbox.ChatCommandManager;

namespace LeagueSandbox.GameServer.Logic.Chatbox.Commands
{
    class CoordsCommand : ChatCommand
    {
        public CoordsCommand(string command, string syntax, ChatCommandManager owner) : base(command, syntax, owner) { }

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
                    "At Coords - X: {0} Y: {1} Z: {2}  Direction - {3}",
                    champion.X,
                    champion.Y,
                    champion.GetZ(),
                    Math.Atan2(champion.Direction.Y, champion.Direction.X) * 180 / Math.PI
                )
            );
        }
    }
}
