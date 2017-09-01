using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class KeyCheckRequest
    {
        public PacketCmd cmd;
        public byte[] partialKey = new byte[3];   //Bytes 1 to 3 from the blowfish key for that client
        public uint playerNo;
        public long userId;         //short testVar[8];   //User id
        public uint trash;
        public ulong checkId;        //short checkVar[8];  //Encrypted testVar
        public uint trash2;

        public KeyCheckRequest(byte[] bytes)
        {
            var reader = new BinaryReader(new MemoryStream(bytes));
            cmd = (PacketCmd)reader.ReadByte();
            partialKey[0] = reader.ReadByte();
            partialKey[1] = reader.ReadByte();
            partialKey[2] = reader.ReadByte();
            playerNo = reader.ReadUInt32();
            userId = reader.ReadInt64();
            trash = reader.ReadUInt32();
            checkId = reader.ReadUInt64();
            if (reader.BaseStream.Position != reader.BaseStream.Length)
            {
                trash2 = reader.ReadUInt32();
            }
            reader.Close();
        }
    }
}