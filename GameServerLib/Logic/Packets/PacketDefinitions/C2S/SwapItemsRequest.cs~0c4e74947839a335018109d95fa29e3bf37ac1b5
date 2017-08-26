using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class SwapItemsRequest : ClientPacketBase
    {
        public byte SlotFrom { get; private set; }
        public byte SlotTo { get; private set; }

        public SwapItemsRequest(byte[] data) : base(data)
        {

        }

        protected override void ParseInternal(BinaryReader reader)
        {
            SlotFrom = reader.ReadByte();
            SlotTo = reader.ReadByte();
        }
    }
}