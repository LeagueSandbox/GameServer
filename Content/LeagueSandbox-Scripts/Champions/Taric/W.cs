using System.Linq;
using GameServerCore;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;

namespace Spells
{
    public class Shatter : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            // TODO
        };

        public void OnActivate(IObjAiBase owner, ISpell spell)
        {

        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {

        }

        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {

        }

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            var armor = owner.Stats.Armor.Total;
            var damage = spell.CastInfo.SpellLevel * 40 + armor * 0.2f;
            var reduce = spell.CastInfo.SpellLevel * 5 + armor * 0.05f;
            AddParticleTarget(owner, "Shatter_nova.troy", owner, 1);

            foreach (var enemy in GetUnitsInRange(owner.Position, 375, true)
                .Where(x => x.Team == CustomConvert.GetEnemyTeam(owner.Team)))
            {
                var hasbuff = HasBuff((IObjAiBase)enemy, "TaricWDis");
                if (enemy is IObjAiBase)
                {
                    enemy.TakeDamage(owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
                    var p2 = AddParticleTarget(owner, "Shatter_tar.troy", enemy, 1);
                    AddBuff("TaricWDis", 4.0f, 1, spell, enemy, owner);

                    if (hasbuff == true)
                    {
                        return;
                    }
                    if (hasbuff == false)
                    {
                        enemy.Stats.Armor.FlatBonus -= reduce;
                    }

                    CreateTimer(4f, () =>
                    {
                        enemy.Stats.Armor.FlatBonus += reduce;
                        RemoveParticle(p2);
                    });
                }
            }
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, ISpellMissile projectile)
        {

        }

        public void OnUpdate(float diff)
        {

        }
    }
}
