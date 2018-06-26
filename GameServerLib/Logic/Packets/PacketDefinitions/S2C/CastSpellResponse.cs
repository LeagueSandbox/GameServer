using System;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class CastSpellResponse : BasePacket
    {
        public CastSpellResponse(Spell s, float x, float y, float xDragEnd, float yDragEnd, uint futureProjNetId, uint spellNetId)
            : base(PacketCmd.PKT_S2C_CastSpellAns, s.Owner.NetId)
        {
            var m = Game.Map;

            buffer.Write(Environment.TickCount); // syncID
            buffer.Write((byte)0); // Unk
            buffer.Write((short)0x66); // Buffer size from here
            buffer.Write((int)s.getId()); // Spell hash, for example hash("EzrealMysticShot")
            buffer.Write((uint)spellNetId); // Spell net ID
            buffer.Write((byte)(s.Level - 1));
            buffer.Write((float)1.0f); // attackSpeedMod
            buffer.Write((uint)s.Owner.NetId);
            buffer.Write((uint)s.Owner.NetId);
            buffer.Write((int)s.Owner.getChampionHash());
            buffer.Write((uint)futureProjNetId); // The projectile ID that will be spawned
            buffer.Write((float)x);
            buffer.Write((float)m.NavGrid.GetHeightAtLocation(x, y));
            buffer.Write((float)y);
            buffer.Write((float)xDragEnd);
            buffer.Write((float)m.NavGrid.GetHeightAtLocation(xDragEnd, yDragEnd));
            buffer.Write((float)yDragEnd);
            buffer.Write((byte)0); // numTargets (if >0, what follows is a list of {uint32 targetNetId, uint8 hitResult})
            buffer.Write((float)s.SpellData.GetCastTime()); // designerCastTime
            buffer.Write((float)0.0f); // extraTimeForCast
            buffer.Write((float)s.SpellData.GetCastTime() /*+ s.ChannelTime*/); // designerTotalTime
            buffer.Write((float)s.GetCooldown());
            buffer.Write((float)0.0f); // startCastTime
            buffer.Write((byte)0); // flags (isAutoAttack, secondAttack, forceCastingOrChannelling, mShouldOverrideCastPosition)
            buffer.Write((byte)s.Slot);
            buffer.Write((float)s.SpellData.ManaCost[s.Level]);
            buffer.Write((float)s.Owner.X);
            buffer.Write((float)s.Owner.GetZ());
            buffer.Write((float)s.Owner.Y);
            buffer.Write((long)1); // Unk
        }
    }
}