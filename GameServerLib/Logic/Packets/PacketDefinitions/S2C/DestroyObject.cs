using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class DestroyObject : BasePacket
    {
        public DestroyObject(Unit destroyer, Unit destroyed)
            : base(PacketCmd.PKT_S2C_DestroyObject, destroyer.NetId)
        {
            buffer.Write((uint)destroyed.NetId);
        }
    }
}