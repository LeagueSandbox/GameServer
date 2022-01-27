using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Linq;
using GameServerCore;
using System.Numerics;
using GameServerCore.Scripting.CSharp;

namespace Spells
{
    public class KarthusLayWasteA1 : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
            // TODO
        };

        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
        }

        public void OnSpellCast(ISpell spell)
        {
            var owner = spell.CastInfo.Owner;
             var spellPos = new Vector2(spell.CastInfo.TargetPosition.X, spell.CastInfo.TargetPosition.Z);
            AddParticleTarget(owner, owner, "Karthus_Base_Q_Hand_Glow", owner, bone: "R_Hand");
            AddParticle(owner, null, "Karthus_Base_Q_Point", spellPos);
            AddParticle(owner, null, "Karthus_Base_Q_Ring", spellPos);
            AddParticle(owner, null, "Karthus_Base_Q_Skull_Child", spellPos);
        }

        public void OnSpellPostCast(ISpell spell)
        {
            var owner = spell.CastInfo.Owner;
            var spellPos = new Vector2(spell.CastInfo.TargetPosition.X, spell.CastInfo.TargetPosition.Z);
            IGameObject m = AddParticle(owner, null, "Karthus_Base_Q_Explosion", spellPos);
            var affectedUnits = GetUnitsInRange(m.Position, 150, true);
            var ap = spell.CastInfo.Owner.Stats.AbilityPower.Total;
            var damage = 20f + spell.CastInfo.SpellLevel * 20f + ap * 0.3f;
            if (affectedUnits.Count == 0)
            {
                AddParticle(owner, null, "Karthus_Base_Q_Hit_Miss", spellPos);
            }
            foreach (var unit in affectedUnits
            .Where(x => x.Team == CustomConvert.GetEnemyTeam(spell.CastInfo.Owner.Team)))
            {
                if (unit is IChampion || unit is IMinion)
                {                        
                    if (affectedUnits.Count == 1)
                    {
                        damage *= 2;
                        AddParticle(owner, null, "Karthus_Base_Q_Hit_Single", spellPos);
                        unit.TakeDamage(spell.CastInfo.Owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, true);
                    }
                    if (affectedUnits.Count > 1)
                    {
                        AddParticle(owner, null, "Karthus_Base_Q_Hit_Many", spellPos);
                        unit.TakeDamage(spell.CastInfo.Owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
                    }
                }
            }
            m.SetToRemove();
            AddParticle(owner, null, "Karthus_Base_Q_Explosion_Sound", spellPos);
        }

        public void OnSpellChannel(ISpell spell)
        {
        }

        public void OnSpellChannelCancel(ISpell spell, ChannelingStopSource reason)
        {
        }

        public void OnSpellPostChannel(ISpell spell)
        {
        }

        public void OnUpdate(float diff)
        {
        }
    }
}
