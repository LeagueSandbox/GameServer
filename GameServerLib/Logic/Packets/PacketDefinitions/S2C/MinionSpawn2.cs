using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class MinionSpawn2 : Packet // shhhhh....
    {
        public MinionSpawn2(uint netId) : base(PacketCmd.PKT_S2C_ObjectSpawn)
        {
            buffer.Write((uint)netId);
            buffer.fill(0, 3);
        }
    }
}