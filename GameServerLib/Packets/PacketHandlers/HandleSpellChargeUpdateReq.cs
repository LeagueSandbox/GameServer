using GameServerCore.Packets.Handlers;
using GameServerCore.Packets.PacketDefinitions.Requests;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleSpellChargeUpdateReq : PacketHandlerBase<SpellChargeUpdateReq>
    {
        private readonly Game _game;

        public HandleSpellChargeUpdateReq(Game game)
        {
            _game = game;
        }

        public override bool HandlePacket(int userId, SpellChargeUpdateReq req)
        {
            // TODO: Implement handling for this request.
            _game.PacketNotifier.NotifyDebugMessage($"X: {req.Position.X} Y: {req.Position.Y} Z: {req.Position.Z}");
            return true;
        }
    }
}
