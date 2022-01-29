using System.Numerics;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects.Stats;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    class LeonaShieldOfDaybreak : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER,
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        IParticle pbuff;
        IBuff thisBuff;

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            thisBuff = buff;
            pbuff = AddParticleTarget(ownerSpell.CastInfo.Owner, unit, "Leona_ShieldOfDaybreak_cas", unit, buff.Duration, bone: "BUFFBONE_CSTM_SHIELD_TOP");

            StatsModifier.Range.FlatBonus = 30.0f;

            unit.AddStatModifier(StatsModifier);

            if (unit is IObjAiBase ai)
            {
                SealSpellSlot(ai, SpellSlotType.SpellSlots, 0, SpellbookType.SPELLBOOK_CHAMPION, true);
                ai.CancelAutoAttack(true);

                ApiEventManager.OnPreAttack.AddListener(this, ai, OnPreAttack, true);
            }
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            if (buff.TimeElapsed >= buff.Duration)
            {
                ApiEventManager.OnPreAttack.RemoveListener(this, unit as IObjAiBase);
            }

            // TODO: Spell Cooldown

            if (unit is IObjAiBase ai)
            {
                SealSpellSlot(ai, SpellSlotType.SpellSlots, 0, SpellbookType.SPELLBOOK_CHAMPION, false);
            }

            RemoveParticle(pbuff);
        }

        public void OnPreAttack(ISpell spell)
        {
            spell.CastInfo.Owner.SkipNextAutoAttack();

            SpellCast(spell.CastInfo.Owner, 0, SpellSlotType.ExtraSlots, false, spell.CastInfo.Owner.TargetUnit, Vector2.Zero);

            if (thisBuff != null)
            {
                thisBuff.DeactivateBuff();
            }
        }

        public void OnUpdate(float diff)
        {
        }
    }
}
