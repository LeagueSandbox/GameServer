using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using System;
using GameServerCore.Enums;

namespace Spells
{
    public class Meditate : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            NotSingleTargetSpell = true,
            TriggersSpellCasts = true,
            ChannelDuration = 4f,
            AutoCooldownByLevel = new float[]
            {
                50f,
                50f,
                50f,
                50f,
                50f
            }
        };

        IObjAiBase Owner;

        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
            Owner = owner;
        }

        public void OnSpellCast(ISpell spell)
        {
            AddParticleTarget(Owner, Owner, "masteryi_base_w_cas", Owner, flags: 0);
        }

        public void OnSpellPostCast(ISpell spell)
        {
        }

        public void OnSpellChannel(ISpell spell)
        {
            AddBuff("Meditate", 4.0f, 1, spell, Owner, Owner);
        }

        public void OnSpellChannelCancel(ISpell spell, ChannelingStopSource reason)
        {
            RemoveBuff(Owner, "Meditate");
        }

        public void OnSpellPostChannel(ISpell spell)
        {
            //float[] finalHeal = new float[]
            //{
            //    25f,
            //    50f,
            //    83.3f,
            //    125f,
            //    183.3f
            //};
            //Owner.Stats.CurrentHealth = Math.Min(Owner.Stats.CurrentHealth, finalHeal[spell.CastInfo.SpellLevel]);
            RemoveBuff(Owner, "Meditate");
        }

        public void OnUpdate(float diff)
        {
        }
    }
}
