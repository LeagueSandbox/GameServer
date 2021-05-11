using System.Linq;
using GameServerCore;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Scripting.CSharp;

namespace Spells
{
    public class Shatter : ISpellScript
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
        }

        public void OnSpellPostCast(ISpell spell)
        {
            var armor = spell.CastInfo.Owner.Stats.Armor.Total;
            var damage = spell.CastInfo.SpellLevel * 40 + armor * 0.2f;
            var reduce = spell.CastInfo.SpellLevel * 5 + armor * 0.05f;
            AddParticleTarget(spell.CastInfo.Owner, "Shatter_nova.troy", spell.CastInfo.Owner, 1);

            foreach (var enemy in GetUnitsInRange(spell.CastInfo.Owner.Position, 375, true)
                .Where(x => x.Team == CustomConvert.GetEnemyTeam(spell.CastInfo.Owner.Team)))
            {
                var hasbuff = HasBuff((IObjAiBase)enemy, "TaricWDis");
                if (enemy is IObjAiBase)
                {
                    enemy.TakeDamage(spell.CastInfo.Owner, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_SPELL, false);
                    var p2 = AddParticleTarget(spell.CastInfo.Owner, "Shatter_tar.troy", enemy, 1);
                    AddBuff("TaricWDis", 4.0f, 1, spell, enemy, spell.CastInfo.Owner);

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

        public void OnSpellChannel(ISpell spell)
        {
        }

        public void OnSpellChannelCancel(ISpell spell)
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
