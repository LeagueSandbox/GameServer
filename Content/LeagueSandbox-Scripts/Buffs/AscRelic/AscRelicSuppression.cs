using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects.Spell;
using LeagueSandbox.GameServer.API;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.GameObjects.Stats;

namespace Buffs
{
    internal class AscRelicSuppression : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.AURA,
            BuffAddType = BuffAddType.REPLACE_EXISTING,
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        IBuff Buff;
        float timer = 100.0f;
        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            Buff = buff;
            ApiEventManager.OnDeath.AddListener(this, unit, OnDeath, true);
        }

        public void OnDeath(IDeathData deathData)
        {
            Buff.DeactivateBuff();
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            ApiEventManager.OnDeath.RemoveListener(this);
            unit.Stats.CurrentMana = unit.Stats.ManaPoints.Total;
        }

        public void OnUpdate(float diff)
        {
            timer -= diff;
            if(timer <= 0)
            {
                //Exact values for both Current Mana reduction and timer are unknow, these are approximations.
                Buff.TargetUnit.Stats.CurrentMana -= 700;
                timer = 530;
            }
        }
    }
}