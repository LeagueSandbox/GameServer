using System.IO;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.C2S
{
    public class UseObject
    {
        private PacketCmd _cmd;
        private int _netId;
        public uint TargetNetId; // netId of the object used

        public UseObject(byte[] data)
        {
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                _cmd = (PacketCmd)reader.ReadByte();
                _netId = reader.ReadInt32();
                TargetNetId = reader.ReadUInt32();
            }
        }
    }
}