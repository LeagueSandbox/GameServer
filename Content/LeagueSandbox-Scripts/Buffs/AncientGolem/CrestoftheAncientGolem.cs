using System.Numerics;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects.Stats;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace Buffs
{
    internal class CrestoftheAncientGolem : IBuffGameScript
    {
        public BuffType BuffType => BuffType.COMBAT_ENCHANCER;
        public BuffAddType BuffAddType => BuffAddType.RENEW_EXISTING;
        public int MaxStacks => 1;
        public bool IsHidden => false;

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        IBuff thisBuff;
        IParticle particle;
        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            thisBuff  = buff;
            ApiEventManager.OnDeath.AddListener(this, unit, OnDeath, false);
            particle = AddParticleTarget(unit, unit, "NeutralMonster_buf_blue_defense_big", unit, buff.Duration);
            StatsModifier.ManaRegeneration.FlatBonus += 5 + unit.Stats.ManaPoints.Total * 0.05f;
            StatsModifier.CooldownReduction.FlatBonus += 0.1f;
            unit.AddStatModifier(StatsModifier);
        }

        public void OnDeath(IDeathData deathData)
        {
            if (deathData.Killer is IChampion)
            {
                thisBuff.DeactivateBuff();
                AddBuff("CrestoftheAncientGolem", 120f, 1, null, deathData.Killer, deathData.Unit as IObjAiBase);
            } 
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            ApiEventManager.OnDeath.RemoveListener(this);
            RemoveParticle(particle);
        }

        public void OnUpdate(float diff)
        {

        }
    }
}
