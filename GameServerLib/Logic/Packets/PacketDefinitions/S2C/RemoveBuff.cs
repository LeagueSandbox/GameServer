using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class RemoveBuff : BasePacket
    {
        public RemoveBuff(Game game, AttackableUnit u, string name, byte slot)
            : base(game, PacketCmd.PKT_S2C_REMOVE_BUFF, u.NetId)
        {
            Write(slot);
            WriteStringHash(name);
            Write(0x0);
            //WriteNetId(u);//source?
        }
    }
}