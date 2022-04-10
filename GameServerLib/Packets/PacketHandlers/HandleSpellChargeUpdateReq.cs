using GameServerCore.Packets.Handlers;
using LeaguePackets.Game;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleSpellChargeUpdateReq : PacketHandlerBase<C2S_SpellChargeUpdateReq>
    {
        private readonly Game _game;

        public HandleSpellChargeUpdateReq(Game game)
        {
            _game = game;
        }

        public override bool HandlePacket(int userId, C2S_SpellChargeUpdateReq req)
        {
            // TODO: Implement handling for this request.
            _game.PacketNotifier.NotifyS2C_SystemMessage($"X: {req.Position.X} Y: {req.Position.Y} Z: {req.Position.Z}");
            return true;
        }
    }
}
