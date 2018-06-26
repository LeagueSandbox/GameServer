using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class UseObject
    {
        PacketCmd _cmd;
        int _netId;
        public uint TargetNetId; // netId of the object used

        public UseObject(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            _cmd = (PacketCmd)reader.ReadByte();
            _netId = reader.ReadInt32();
            TargetNetId = reader.ReadUInt32();
        }
    }
}