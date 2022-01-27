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
    internal class BlessingoftheLizardElder : IBuffGameScript
    {
        public BuffType BuffType => BuffType.COMBAT_ENCHANCER;
        public BuffAddType BuffAddType => BuffAddType.RENEW_EXISTING;
        public int MaxStacks => 1;
        public bool IsHidden => false;

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        IBuff thisBuff;
        IObjAiBase Obj;
        IParticle particle;
        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            thisBuff  = buff;
            if(unit is IObjAiBase obj)
            {
                Obj = obj;
                ApiEventManager.OnDeath.AddListener(this, obj, OnDeath, false);
                if(!(unit is IMonster))
                {
                    ApiEventManager.OnHitUnit.AddListener(this, obj, OnHitUnit, false);
                }
            }

            particle = AddParticleTarget(unit, unit, "NeutralMonster_buf_red_offense_big", unit, buff.Duration);
        }

        public void OnDeath(IDeathData deathData)
        {
            if (deathData.Killer is IChampion)
            {
                thisBuff.DeactivateBuff();
                AddBuff("BlessingoftheLizardElder", 120f, 1, null, deathData.Killer, deathData.Unit as IObjAiBase);
            } 
        }
        public void OnHitUnit(IAttackableUnit target, bool IsCrit)
        {
            if(!(target is IBaseTurret || target is IObjAnimatedBuilding || target is IObjBuilding))
            {
                AddBuff("Burning", 3.0f, 1, null, target, Obj);
            }
        }
        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            ApiEventManager.OnDeath.RemoveListener(this);
            ApiEventManager.OnHitUnit.RemoveListener(this);
            RemoveParticle(particle);
        }

        public void OnUpdate(float diff)
        {

        }
    }
}
