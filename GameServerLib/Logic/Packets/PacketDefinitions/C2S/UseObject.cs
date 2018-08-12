using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class UseObject
    {
        PacketCmd cmd;
        int netId;
        public uint targetNetId; // netId of the object used

        public UseObject(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmd)reader.ReadByte();
            netId = reader.ReadInt32();
            targetNetId = reader.ReadUInt32();
        }
    }
}