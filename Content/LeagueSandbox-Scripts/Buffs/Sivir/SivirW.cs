using System.Numerics;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.SpellNS.Missile;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    class SivirW : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER,
            BuffAddType = BuffAddType.STACKS_AND_RENEWS,
            MaxStacks = 3
        };

        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        Particle pbuff;
        Buff thisBuff;

        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            thisBuff = buff;
            pbuff = AddParticleTarget(ownerSpell.CastInfo.Owner, unit, "Sivir_Base_W_Buff", unit, buff.Duration, bone: "BUFFBONE_CSTM_WEAPON_1");

            if (unit is ObjAIBase ai)
            {
                SealSpellSlot(ai, SpellSlotType.SpellSlots, 1, SpellbookType.SPELLBOOK_CHAMPION, true);
                ai.CancelAutoAttack(true);

                ApiEventManager.OnLaunchMissile.AddListener(this, ai.AutoAttackSpell, OnLaunchMissile, false);
            }
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            if (buff.TimeElapsed >= buff.Duration)
            {
                ApiEventManager.OnLaunchMissile.RemoveListener(this);
            }

            // TODO: Spell Cooldown

            if (unit is ObjAIBase ai)
            {
                SealSpellSlot(ai, SpellSlotType.SpellSlots, 1, SpellbookType.SPELLBOOK_CHAMPION, false);
            }

            RemoveParticle(pbuff);
        }

        public void OnLaunchMissile(Spell spell, SpellMissile missile)
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
    }
}
