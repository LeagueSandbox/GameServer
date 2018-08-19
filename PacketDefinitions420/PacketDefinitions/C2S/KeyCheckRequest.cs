using System.IO;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.C2S
{
    public class KeyCheckRequest
    {
        public PacketCmd Cmd;
        public byte[] PartialKey = new byte[3];   //Bytes 1 to 3 from the blowfish key for that client
        public uint PlayerNo;
        public long UserId;         //short testVar[8];   //User id
        public uint VersionNo;
        public ulong CheckId;        //short checkVar[8];  //Encrypted testVar
        public uint Trash2;

        public KeyCheckRequest(byte[] bytes)
        {
            using (var reader = new BinaryReader(new MemoryStream(bytes)))
            {
                Cmd = (PacketCmd)reader.ReadByte();
                PartialKey[0] = reader.ReadByte();
                PartialKey[1] = reader.ReadByte();
                PartialKey[2] = reader.ReadByte();
                PlayerNo = reader.ReadUInt32();
                UserId = reader.ReadInt64();
                VersionNo = reader.ReadUInt32();
                CheckId = reader.ReadUInt64();
                if (reader.BaseStream.Position != reader.BaseStream.Length)
                {
                    Trash2 = reader.ReadUInt32();
                }
            }
        }
    }
}