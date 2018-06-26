using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class FogUpdate2 : BasePacket
    {
        public FogUpdate2(AttackableUnit unit, NetworkIdManager idManager) : base(PacketCmd.PKT_S2_C_FOG_UPDATE2, 0)
        {
            _buffer.Write((int)unit.Team);
            _buffer.Write((byte)0xFE);
            _buffer.Write((byte)0xFF);
            _buffer.Write((byte)0xFF);
            _buffer.Write((byte)0xFF);
            _buffer.Write((int)0);
            _buffer.Write((uint)unit.NetId); // Fog Attached, when unit dies it disappears
            _buffer.Write((uint)idManager.GetNewNetId()); //Fog NetID
            _buffer.Write((int)0);
            _buffer.Write((float)unit.X);
            _buffer.Write((float)unit.Y);
            _buffer.Write((float)2500);
            _buffer.Write((float)88.4f);
            _buffer.Write((float)130);
            _buffer.Write((float)1.0f);
            _buffer.Write((int)0);
            _buffer.Write((byte)199);
            _buffer.Write((float)unit.VisionRadius);
        }
    }
}