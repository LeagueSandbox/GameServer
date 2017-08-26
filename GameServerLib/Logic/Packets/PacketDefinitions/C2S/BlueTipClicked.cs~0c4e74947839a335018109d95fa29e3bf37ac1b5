using System.IO;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class BlueTipClicked : ClientPacketBase
    {
        public byte Unk { get; private set; }
        public uint TargetNetId { get; private set; }

        public BlueTipClicked(byte[] data) : base(data)
        {

        }

        protected override void ParseInternal(BinaryReader reader)
        {
            Unk = reader.ReadByte();
            TargetNetId = reader.ReadUInt32();
        }
    }
}