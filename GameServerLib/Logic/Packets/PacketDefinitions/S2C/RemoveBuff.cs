using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class RemoveBuff : BasePacket
    {
        public RemoveBuff(AttackableUnit u, string name, byte slot) 
            : base(PacketCmd.PKT_S2C_RemoveBuff, u.NetId)
        {
            buffer.Write((byte)slot);
            buffer.Write(HashFunctions.HashString(name));
            buffer.Write((int)0x0);
            //buffer.Write(u.NetId);//source?
        }
    }
}