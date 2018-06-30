using System;
using LeagueSandbox.GameServer.Logic.GameObjects.Spells;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class CastSpellResponse : BasePacket
    {
        public CastSpellResponse(Spell s, float x, float y, float xDragEnd, float yDragEnd, uint futureProjNetId, uint spellNetId)
            : base(PacketCmd.PKT_S2C_CAST_SPELL_ANS, s.Owner.NetId)
        {
            var m = Game.Map;

            _buffer.Write(Environment.TickCount); // syncID
            _buffer.Write((byte)0); // Unk
            _buffer.Write((short)0x66); // Buffer size from here
            _buffer.Write((int)s.GetId()); // Spell hash, for example hash("EzrealMysticShot")
            _buffer.Write((uint)spellNetId); // Spell net ID
            _buffer.Write((byte)(s.Level - 1));
            _buffer.Write(1.0f); // attackSpeedMod
            _buffer.Write((uint)s.Owner.NetId);
            _buffer.Write((uint)s.Owner.NetId);
            _buffer.Write((int)s.Owner.GetChampionHash());
            _buffer.Write((uint)futureProjNetId); // The projectile ID that will be spawned
            _buffer.Write((float)x);
            _buffer.Write((float)m.NavGrid.GetHeightAtLocation(x, y));
            _buffer.Write((float)y);
            _buffer.Write((float)xDragEnd);
            _buffer.Write((float)m.NavGrid.GetHeightAtLocation(xDragEnd, yDragEnd));
            _buffer.Write((float)yDragEnd);
            _buffer.Write((byte)0); // numTargets (if >0, what follows is a list of {uint32 targetNetId, uint8 hitResult})
            _buffer.Write((float)s.SpellData.GetCastTime()); // designerCastTime
            _buffer.Write(0.0f); // extraTimeForCast
            _buffer.Write((float)s.SpellData.GetCastTime() /*+ s.ChannelTime*/); // designerTotalTime
            _buffer.Write((float)s.GetCooldown());
            _buffer.Write(0.0f); // startCastTime
            _buffer.Write((byte)0); // flags (isAutoAttack, secondAttack, forceCastingOrChannelling, mShouldOverrideCastPosition)
            _buffer.Write((byte)s.Slot);
            _buffer.Write((float)s.SpellData.ManaCost[s.Level]);
            _buffer.Write((float)s.Owner.X);
            _buffer.Write((float)s.Owner.GetZ());
            _buffer.Write((float)s.Owner.Y);
            _buffer.Write((long)1); // Unk
        }
    }
}