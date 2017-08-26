using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class UseObject : ClientPacketBase
    {
        public uint TargetNetId { get; private set; } // netId of the object used

        public UseObject(byte[] data) : base(data)
        {

        }

        protected override void ParseInternal(BinaryReader reader)
        {
            TargetNetId = reader.ReadUInt32();
        }
    }
}