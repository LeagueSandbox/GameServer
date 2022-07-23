using System.Numerics;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    class LeonaShieldOfDaybreak : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER,
        };

        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        Particle pbuff;
        Buff thisBuff;

        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            thisBuff = buff;
            pbuff = AddParticleTarget(ownerSpell.CastInfo.Owner, unit, "Leona_ShieldOfDaybreak_cas", unit, buff.Duration, bone: "BUFFBONE_CSTM_SHIELD_TOP");

            StatsModifier.Range.FlatBonus = 30.0f;

            unit.AddStatModifier(StatsModifier);

            if (unit is ObjAIBase ai)
            {
                SealSpellSlot(ai, SpellSlotType.SpellSlots, 0, SpellbookType.SPELLBOOK_CHAMPION, true);
                ai.CancelAutoAttack(true);

                ApiEventManager.OnPreAttack.AddListener(this, ai, OnPreAttack, true);
            }
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            if (buff.TimeElapsed >= buff.Duration)
            {
                ApiEventManager.OnPreAttack.RemoveListener(this, unit as ObjAIBase);
            }

            // TODO: Spell Cooldown

            if (unit is ObjAIBase ai)
            {
                SealSpellSlot(ai, SpellSlotType.SpellSlots, 0, SpellbookType.SPELLBOOK_CHAMPION, false);
            }

            RemoveParticle(pbuff);
        }

        public void OnPreAttack(Spell spell)
        {
            spell.CastInfo.Owner.SkipNextAutoAttack();

            SpellCast(spell.CastInfo.Owner, 0, SpellSlotType.ExtraSlots, false, spell.CastInfo.Owner.TargetUnit, Vector2.Zero);

            if (thisBuff != null)
            {
                thisBuff.DeactivateBuff();
            }
        }
    }
}
