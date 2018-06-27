using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class MinionSpawn2 : Packet // shhhhh....
    {
        public MinionSpawn2(uint netId) : base(PacketCmd.PKT_S2_C_OBJECT_SPAWN)
        {
            _buffer.Write(netId);
            _buffer.Fill(0, 3);
        }
    }
}