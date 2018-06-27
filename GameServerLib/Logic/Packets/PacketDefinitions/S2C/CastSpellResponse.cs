using System;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.Spells;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class CastSpellResponse : BasePacket
    {
        public CastSpellResponse(Spell s, float x, float y, float xDragEnd, float yDragEnd, uint futureProjNetId, uint spellNetId)
            : base(PacketCmd.PKT_S2_C_CAST_SPELL_ANS, s.Owner.NetId)
        {
            var m = Game.Map;

            _buffer.Write(Environment.TickCount); // syncID
            _buffer.Write((byte)0); // Unk
            _buffer.Write((short)0x66); // Buffer size from here
            _buffer.Write(s.GetId()); // Spell hash, for example hash("EzrealMysticShot")
            _buffer.Write(spellNetId); // Spell net ID
            _buffer.Write((byte)(s.Level - 1));
            _buffer.Write(1.0f); // attackSpeedMod
            _buffer.Write(s.Owner.NetId);
            _buffer.Write(s.Owner.NetId);
            _buffer.Write(s.Owner.GetChampionHash());
            _buffer.Write(futureProjNetId); // The projectile ID that will be spawned
            _buffer.Write(x);
            _buffer.Write(m.NavGrid.GetHeightAtLocation(x, y));
            _buffer.Write(y);
            _buffer.Write(xDragEnd);
            _buffer.Write(m.NavGrid.GetHeightAtLocation(xDragEnd, yDragEnd));
            _buffer.Write(yDragEnd);
            _buffer.Write((byte)0); // numTargets (if >0, what follows is a list of {uint32 targetNetId, uint8 hitResult})
            _buffer.Write(s.SpellData.GetCastTime()); // designerCastTime
            _buffer.Write(0.0f); // extraTimeForCast
            _buffer.Write(s.SpellData.GetCastTime() /*+ s.ChannelTime*/); // designerTotalTime
            _buffer.Write(s.GetCooldown());
            _buffer.Write(0.0f); // startCastTime
            _buffer.Write((byte)0); // flags (isAutoAttack, secondAttack, forceCastingOrChannelling, mShouldOverrideCastPosition)
            _buffer.Write(s.Slot);
            _buffer.Write(s.SpellData.ManaCost[s.Level]);
            _buffer.Write(s.Owner.X);
            _buffer.Write(s.Owner.GetZ());
            _buffer.Write(s.Owner.Y);
            _buffer.Write((long)1); // Unk
        }
    }
}