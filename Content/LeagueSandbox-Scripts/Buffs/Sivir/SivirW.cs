using System.Numerics;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects.Stats;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    class SivirW : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER,
            BuffAddType = BuffAddType.STACKS_AND_RENEWS,
            MaxStacks = 3
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        IParticle pbuff;
        IBuff thisBuff;

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            thisBuff = buff;
            pbuff = AddParticleTarget(ownerSpell.CastInfo.Owner, unit, "Sivir_Base_W_Buff", unit, buff.Duration, bone: "BUFFBONE_CSTM_WEAPON_1");

            if (unit is IObjAiBase ai)
            {
                SealSpellSlot(ai, SpellSlotType.SpellSlots, 1, SpellbookType.SPELLBOOK_CHAMPION, true);
                ai.CancelAutoAttack(true);

                ApiEventManager.OnLaunchMissile.AddListener(this, new System.Collections.Generic.KeyValuePair<IObjAiBase, ISpell>(ai, ai.AutoAttackSpell), OnLaunchMissile, false);
            }
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            if (buff.TimeElapsed >= buff.Duration)
            {
                ApiEventManager.OnLaunchMissile.RemoveListener(this);
            }

            // TODO: Spell Cooldown

            if (unit is IObjAiBase ai)
            {
                SealSpellSlot(ai, SpellSlotType.SpellSlots, 1, SpellbookType.SPELLBOOK_CHAMPION, false);
            }

            RemoveParticle(pbuff);
        }

        public void OnLaunchMissile(ISpell spell, ISpellMissile missile)
        {
            if (thisBuff != null && thisBuff.StackCount != 0 && !thisBuff.Elapsed())
            {
                // Remove the auto attack missile.
                missile.SetToRemove();
                // Decrement stack count.
                spell.CastInfo.Owner.RemoveBuff(thisBuff);
                // Cast the SivirWAttack (without casting, so just the missile).
                SpellCast(spell.CastInfo.Owner, 0, SpellSlotType.ExtraSlots, true, spell.CastInfo.Targets[0].Unit, Vector2.Zero);
            }
        }

        public void OnUpdate(float diff)
        {
        }
    }
}
