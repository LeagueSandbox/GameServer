using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketArgs;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class FogUpdate2 : BasePacket
    {
        public FogUpdate2(FogUpdate2Args args) : base(PacketCmd.PKT_S2C_FogUpdate2, 0)
        {
            buffer.Write((int)args.UnitTeam);
            buffer.Write((byte)0xFE);
            buffer.Write((byte)0xFF);
            buffer.Write((byte)0xFF);
            buffer.Write((byte)0xFF);
            buffer.Write((int)0);
            buffer.Write((uint)args.Unit.UnitNetId); // Fog Attached, when unit dies it disappears
            buffer.Write((uint)args.FogNetId); //Fog NetID
            buffer.Write((int)0);
            buffer.Write((float)args.Unit.X);
            buffer.Write((float)args.Unit.Y);
            buffer.Write((float)2500);
            buffer.Write((float)88.4f);
            buffer.Write((float)130);
            buffer.Write((float)1.0f);
            buffer.Write((int)0);
            buffer.Write((byte)199);
            buffer.Write((float)args.UnitVisionRadius);
        }
    }
}