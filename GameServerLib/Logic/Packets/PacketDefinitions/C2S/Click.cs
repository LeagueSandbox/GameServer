using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class Click
    {
        private PacketCmd _cmd;
        private int _netId;
        public int Zero;
        public uint TargetNetId; // netId on which the player clicked

        public Click(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            _cmd = (PacketCmd)reader.ReadByte();
            _netId = reader.ReadInt32();
            Zero = reader.ReadInt32();
            TargetNetId = reader.ReadUInt32();
        }
    }
}