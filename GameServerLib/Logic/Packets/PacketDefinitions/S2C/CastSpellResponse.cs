using System;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketArgs;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class CastSpellResponse : BasePacket
    {
        public CastSpellResponse(CastSpellResponseArgs args)
            : base(PacketCmd.PKT_S2C_CastSpellAns, args.Owner.UnitNetId)
        {
            var m = Game.Map;

            buffer.Write(Environment.TickCount); // syncID
            buffer.Write((byte)0); // Unk
            buffer.Write((short)0x66); // Buffer size from here
            buffer.Write((int)args.Hash); // Spell hash, for example hash("EzrealMysticShot")
            buffer.Write((uint)args.SpellNetId); // Spell net ID
            buffer.Write((byte)(args.Level - 1));
            buffer.Write((float)1.0f); // attackSpeedMod
            buffer.Write((uint)args.Owner.UnitNetId);
            buffer.Write((uint)args.Owner.UnitNetId);
            buffer.Write((int)args.Owner.UnitHash);
            buffer.Write((uint)args.FutureProjNetId); // The projectile ID that will be spawned
            buffer.Write((float)args.X);
            buffer.Write((float)m.AIMesh.GetHeightAtLocation(args.X, args.Y));
            buffer.Write((float)args.Y);
            buffer.Write((float)args.XDragEnd);
            buffer.Write((float)m.AIMesh.GetHeightAtLocation(args.XDragEnd, args.YDragEnd));
            buffer.Write((float)args.YDragEnd);
            buffer.Write((byte)0); // numTargets (if >0, what follows is a list of {uint32 targetNetId, uint8 hitResult})
            buffer.Write((float)args.CastTime); // designerCastTime
            buffer.Write((float)0.0f); // extraTimeForCast
            buffer.Write((float)args.CastTime /*+ s.ChannelTime*/); // designerTotalTime
            buffer.Write((float)args.CoolDown);
            buffer.Write((float)0.0f); // startCastTime
            buffer.Write((byte)0); // flags (isAutoAttack, secondAttack, forceCastingOrChannelling, mShouldOverrideCastPosition)
            buffer.Write((byte)args.Slot);
            buffer.Write((float)args.ManaCost);
            buffer.Write((float)args.Owner.X);
            buffer.Write((float)args.Owner.Z);
            buffer.Write((float)args.Owner.Y);
            buffer.Write((long)1); // Unk
        }
    }
}