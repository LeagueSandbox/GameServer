using System.IO;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets
{
    public class SwapItems : BasePacket
    {
        public PacketCmd cmd;
        public int netId;
        public byte slotFrom;
        public byte slotTo;
        public SwapItems(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmd)reader.ReadByte();
            netId = reader.ReadInt32();
            slotFrom = reader.ReadByte();
            slotTo = reader.ReadByte();
        }

        public SwapItems(Champion c, byte slotFrom, byte slotTo) : base(PacketCmd.PKT_S2C_SwapItems, c.NetId)
        {
            buffer.Write((byte)slotFrom);
            buffer.Write((byte)slotTo);
        }
    }
}