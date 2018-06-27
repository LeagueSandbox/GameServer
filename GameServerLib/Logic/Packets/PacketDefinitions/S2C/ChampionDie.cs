using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class ChampionDie : BasePacket
    {
        public ChampionDie(Champion die, AttackableUnit killer, int goldFromKill)
            : base(PacketCmd.PKT_S2_C_CHAMPION_DIE, die.NetId)
        {
            _buffer.Write(goldFromKill); // Gold from kill?
            _buffer.Write((byte)0);
            if (killer != null)
                _buffer.Write(killer.NetId);
            else
                _buffer.Write(0);

            _buffer.Write((byte)0);
            _buffer.Write((byte)7);
            _buffer.Write(die.RespawnTimer / 1000.0f); // Respawn timer, float
        }
    }
}