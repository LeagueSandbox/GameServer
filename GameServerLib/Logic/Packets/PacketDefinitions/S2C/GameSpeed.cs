using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class GameSpeed : BasePacket
    {
        public GameSpeed(Game game, float gameSpeed)
            : base(game, PacketCmd.PKT_S2C_GAME_SPEED)
        {
            Write(gameSpeed);
        }
    }
}