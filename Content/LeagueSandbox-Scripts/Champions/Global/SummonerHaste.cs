using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;

namespace Spells
{
    public class SummonerHaste : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            // TODO
        };

        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            AddBuff("SummonerHasteBuff", 10.0f, 1, spell, owner, owner);
            var p1 = AddParticleTarget(owner, "Global_SS_Ghost.troy", target);
            var p2 = AddParticleTarget(owner, "Global_SS_Ghost_cas.troy", target);
            CreateTimer(10.0f, () =>
            {
                RemoveParticle(p1);
                RemoveParticle(p2);
            });
        }

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, ISpellMissile projectile)
        {
        }

        public void OnUpdate(float diff)
        {
        }

        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }
    }
}

