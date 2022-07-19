using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.API;
using GameServerCore.Scripting.CSharp;
using GameServerCore.Enums;
using System.Collections.Generic;
using LeagueSandbox.GameServer.GameObjects;
using            GameServerLib.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;

namespace Spells
{
    public class AscRelicCaptureChannel : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            NotSingleTargetSpell = true,
            DoesntBreakShields = true,
            TriggersSpellCasts = true,
            CastingBreaksStealth = true,
            IsDamagingSpell = true,
            ChannelDuration = 7.0f
        };

        Spell Spell;
        AttackableUnit Target;
        ObjAIBase Owner;
        public void OnActivate(ObjAIBase owner, Spell spell)
        {
            Spell = spell;
            Owner = owner;
        }

        public void OnSpellPreCast(ObjAIBase owner, Spell spell, AttackableUnit target, Vector2 start, Vector2 end)
        {
            Target = target;
        }

        public void OnSpellCast(Spell spell)
        {
            var owner = spell.CastInfo.Owner;
            AddBuff("AscRelicCaptureChannel", 30.0f, 1, spell, owner, owner);

            PlayAnimation(owner, "channel_windup", flags: (AnimationFlags)2);
            ApiEventManager.OnTakeDamage.AddListener(this, owner, OnTakeDamage, true);
        }

        public void OnSpellChannelCancel(Spell spell, ChannelingStopSource reason)
        {
            var target = spell.CastInfo.Targets[0].Unit;
            if (target.HasBuff("AscRelicSuppression"))
            {
                var buffs = spell.CastInfo.Targets[0].Unit.GetBuffsWithName("AscRelicSuppression");
                var ownerBuff = buffs.Find(x => x.SourceUnit == spell.CastInfo.Owner);

                if (ownerBuff != null)
                {
                    ownerBuff.DeactivateBuff();
                }
            }

            AddParticleTarget(Owner, Owner, "OdinCaptureCancel", Owner, 1, 1, "spine", "spine");
            AddParticleTarget(Owner, Owner, "ezreal_essenceflux_tar", Owner, 1, 1, "ROOT");

            StopChannel();
        }

        public void OnSpellPostChannel(Spell spell)
        {
            if (spell.CastInfo.Targets[0].Unit != null)
            {
                var crystal = spell.CastInfo.Targets[0].Unit;
                //I Suspect that the condition that actually kills the crystal is it running out of mana, have to investigate further.
                crystal.Die(CreateDeathData(false, 0, crystal, crystal, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_INTERNALRAW, 0.0f));

                if (spell.CastInfo.Owner is Champion ch)
                {
                    ch.IncrementScore(3.0f, ScoreCategory.Objective, ScoreEvent.MajorRelicPickup, true, true);
                }
            }

            StopChannel();
        }

        public void StopChannel()
        {
            List<Buff> buffs = new List<Buff>
            {
                Target.GetBuffWithName("AscRelicSuppression"),
                Target.GetBuffWithName("OdinBombSuppressionOrder"),
                Target.GetBuffWithName("OdinBombSuppressionChaos"),
                Spell.CastInfo.Owner.GetBuffWithName("AscRelicCaptureChannel"),
                Spell.CastInfo.Owner.GetBuffWithName("OdinChannelVision")
            };

            foreach(var buff in buffs)
            {
                if (buff != null)
                {
                    buff.DeactivateBuff();
                }
            }

            Owner.StopAnimation("", true, true);
            ApiEventManager.OnTakeDamage.RemoveListener(this);
            Spell.SetCooldown(4.0f, true);
        }

        public void OnTakeDamage(DamageData damageData)
        {
            Spell.StopChanneling(ChannelingStopCondition.Cancel, ChannelingStopSource.Attack);
        }
    }
}

