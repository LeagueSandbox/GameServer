using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class RemoveBuff : BasePacket
    {
        public RemoveBuff(AttackableUnit u, string name, byte slot) 
            : base(PacketCmd.PKT_S2_C_REMOVE_BUFF, u.NetId)
        {
            _buffer.Write(slot);
            _buffer.Write(HashFunctions.HashString(name));
            _buffer.Write(0x0);
            //buffer.Write(u.NetId);//source?
        }
    }
}