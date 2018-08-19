using System.IO;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.C2S
{
    public class Click
    {
        private PacketCmd _cmd;
        private int _netId;
        public int Zero;
        public uint TargetNetId; // netId on which the player clicked

        public Click(byte[] data)
        {
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                _cmd = (PacketCmd)reader.ReadByte();
                _netId = reader.ReadInt32();
                Zero = reader.ReadInt32();
                TargetNetId = reader.ReadUInt32();
            }
        }
    }
}