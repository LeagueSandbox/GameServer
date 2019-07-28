using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.API;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Spells
{
    public class SummonerMana : IGameScript
    {
        private const float PERCENT_MAX_MANA_HEAL = 0.40f;

        public void OnStartCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
        }

        public void OnFinishCasting(IChampion owner, ISpell spell, IAttackableUnit target)
        {
            foreach (var unit in GetChampionsInRange(owner, 600, true))
            {
                if (unit.Team == owner.Team)
                {
                    RestoreMana(owner);
                }
            }
        }

        private void RestoreMana(IChampion target)
        {
            
            var maxMp = target.Stats.ManaPoints.Total;
            var newMp = target.Stats.CurrentMana + (maxMp * PERCENT_MAX_MANA_HEAL);
            if (newMp < maxMp)
                target.Stats.CurrentMana = newMp;
            else
                target.Stats.CurrentMana = maxMp;
            AddParticleTarget(target, "global_ss_clarity_02.troy", target);
        }

        public void ApplyEffects(IChampion owner, IAttackableUnit target, ISpell spell, IProjectile projectile)
        {
        }

        public void OnUpdate(double diff)
        {
        }

        public void OnActivate(IChampion owner)
        {
        }

        public void OnDeactivate(IChampion owner)
        {
        }
    }
}

