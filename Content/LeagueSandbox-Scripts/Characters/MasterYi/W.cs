using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;

namespace Spells
{
    public class Meditate : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
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

        ObjAIBase Owner;

        public void OnSpellPreCast(ObjAIBase owner, Spell spell, AttackableUnit target, Vector2 start, Vector2 end)
        {
            Owner = owner;
        }

        public void OnSpellCast(Spell spell)
        {
            AddParticleTarget(Owner, Owner, "masteryi_base_w_cas", Owner, flags: 0);
        }

        public void OnSpellChannel(Spell spell)
        {
            AddBuff("Meditate", 4.0f, 1, spell, Owner, Owner);
        }

        public void OnSpellChannelCancel(Spell spell, ChannelingStopSource reason)
        {
            RemoveBuff(Owner, "Meditate");
        }

        public void OnSpellPostChannel(Spell spell)
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
    }
}
