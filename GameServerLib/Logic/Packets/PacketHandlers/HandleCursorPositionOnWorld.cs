using ENet;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleCursorPositionOnWorld : PacketHandlerBase
    {
        public override PacketCmd PacketType => PacketCmd.PKT_C2S_CURSOR_POSITION_ON_WORLD;
        public override Channel PacketChannel => Channel.CHL_C2_S;
        
        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var cursorPosition = new CursorPositionOnWorld(data);
            var response = new DebugMessage($"X: {cursorPosition.X} Y: {cursorPosition.Y}");

            return Game.PacketHandlerManager.BroadcastPacket(response, Channel.CHL_S2_C);
        }
    }
}
