using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;

namespace Spells
{
    public class SummonerMana : IGameScript
    {
        private const float PERCENT_MAX_MANA_HEAL = 0.40f;

        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
        }

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            foreach (var unit in GetChampionsInRange(owner.Position, 600, true))
            {
                if (unit.Team == owner.Team)
                {
                    RestoreMana(owner);
                }
            }
        }

        private void RestoreMana(IObjAiBase target)
        {

            var maxMp = target.Stats.ManaPoints.Total;
            var newMp = target.Stats.CurrentMana + (maxMp * PERCENT_MAX_MANA_HEAL);
            if (newMp < maxMp)
                target.Stats.CurrentMana = newMp;
            else
                target.Stats.CurrentMana = maxMp;
            AddParticleTarget(target, "global_ss_clarity_02.troy", target);
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, ISpellMissile projectile)
        {
        }

        public void OnUpdate(double diff)
        {
        }

        public void OnActivate(IObjAiBase owner)
        {
        }

        public void OnDeactivate(IObjAiBase owner)
        {
        }
    }
}

