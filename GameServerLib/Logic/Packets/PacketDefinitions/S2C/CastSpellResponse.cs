using System;
using LeagueSandbox.GameServer.Logic.GameObjects.Spells;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class CastSpellResponse : BasePacket
    {
        public CastSpellResponse(Game game, Spell s, float x, float y, float xDragEnd, float yDragEnd, uint futureProjNetId, uint spellNetId)
            : base(game, PacketCmd.PKT_S2C_CAST_SPELL_ANS, s.Owner.NetId)
        {
            var m = Game.Map;

            Write(Environment.TickCount); // syncID
            Write((byte)0); // Unk
            Write((short)0x66); // Buffer size from here
            Write((int)s.GetId()); // Spell hash, for example hash("EzrealMysticShot")
            Write((uint)spellNetId); // Spell net ID
            Write((byte)(s.Level - 1));
            Write(1.0f); // attackSpeedMod
            WriteNetId(s.Owner);
            WriteNetId(s.Owner);
            Write((int)s.Owner.GetChampionHash());
            Write((uint)futureProjNetId); // The projectile ID that will be spawned
            Write((float)x);
            Write((float)m.NavGrid.GetHeightAtLocation(x, y));
            Write((float)y);
            Write((float)xDragEnd);
            Write((float)m.NavGrid.GetHeightAtLocation(xDragEnd, yDragEnd));
            Write((float)yDragEnd);
            Write((byte)0); // numTargets (if >0, what follows is a list of {uint32 targetNetId, uint8 hitResult})
            Write((float)s.SpellData.GetCastTime()); // designerCastTime
            Write(0.0f); // extraTimeForCast
            Write((float)s.SpellData.GetCastTime() /*+ s.ChannelTime*/); // designerTotalTime
            Write((float)s.GetCooldown());
            Write(0.0f); // startCastTime
            Write((byte)0); // flags (isAutoAttack, secondAttack, forceCastingOrChannelling, mShouldOverrideCastPosition)
            Write((byte)s.Slot);
            Write((float)s.SpellData.ManaCost[s.Level]);
            Write((float)s.Owner.X);
            Write((float)s.Owner.GetZ());
            Write((float)s.Owner.Y);
            Write((long)1); // Unk
        }
    }
}