using System.Text;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class PlaySound : BasePacket
    {
        public PlaySound(AttackableUnit unit, string soundName)
            : base(PacketCmd.PKT_S2_C_PLAY_SOUND, unit.NetId)
        {
            _buffer.Write(Encoding.Default.GetBytes(soundName));
            _buffer.Fill(0, 1024 - soundName.Length);
            _buffer.Write(unit.NetId); // audioEventNetworkID?
        }
    }
}