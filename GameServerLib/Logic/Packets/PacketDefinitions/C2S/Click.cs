using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class Click
    {
        PacketCmd cmd;
        int netId;
        public int zero;
        public uint targetNetId; // netId on which the player clicked

        public Click(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmd)reader.ReadByte();
            netId = reader.ReadInt32();
            zero = reader.ReadInt32();
            targetNetId = reader.ReadUInt32();
        }
    }
}