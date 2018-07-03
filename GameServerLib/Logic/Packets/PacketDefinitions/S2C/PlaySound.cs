using System.Text;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class PlaySound : BasePacket
    {
        public PlaySound(AttackableUnit unit, string soundName)
            : base(PacketCmd.PKT_S2C_PLAY_SOUND, unit.NetId)
        {
            Write(Encoding.Default.GetBytes(soundName));
            Fill(0, 1024 - soundName.Length);
            Write(unit.NetId); // audioEventNetworkID?
        }
    }
}