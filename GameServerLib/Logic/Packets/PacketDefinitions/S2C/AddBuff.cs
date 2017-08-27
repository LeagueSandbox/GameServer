using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketArgs;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class AddBuff : BasePacket
    {
        public AddBuff(AddBuffArgs args)
            : base(PacketCmd.PKT_S2C_AddBuff, args.TargetNetId)
        {
            buffer.Write((byte)args.Slot); //Slot
            buffer.Write((byte)args.BuffType); //Type
            buffer.Write((byte)args.Stacks); // stacks
            buffer.Write((byte)0x00); // Visible was (byte)0x00
            buffer.Write(HashFunctions.HashString(args.Name)); //Buff id

            buffer.Write((int)0); // <-- Probably runningTime

            buffer.Write((float)0); // <- ProgressStartPercent

            buffer.Write((float)args.Time);
            buffer.Write(args.SourceNetId); //source
        }
    }
}