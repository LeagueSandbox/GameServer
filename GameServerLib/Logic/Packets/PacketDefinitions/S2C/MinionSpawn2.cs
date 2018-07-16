using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class MinionSpawn2 : Packet // shhhhh....
    {
        public MinionSpawn2(Game game, uint netId) 
            : base(game, PacketCmd.PKT_S2C_OBJECT_SPAWN)
        {
            Write(netId);
            Fill(0, 3);
        }
    }
}