﻿using GameServerCore.Packets.Handlers;
using GameServerCore.Packets.PacketDefinitions.Requests;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleView : PacketHandlerBase<ViewRequest>
    {
        private readonly Game _game;

        public HandleView(Game game)
        {
            _game = game;
        }

        public override bool HandlePacket(int userId, ViewRequest req)
        {
             _game.PacketNotifier.NotifyViewResponse(userId, req);
            return true;
        }
    }
}
