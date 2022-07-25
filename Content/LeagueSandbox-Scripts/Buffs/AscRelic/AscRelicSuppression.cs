using GameServerCore.Enums;
using LeagueSandbox.GameServer.API;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using GameServerLib.GameObjects.AttackableUnits;

namespace Buffs
{
    internal class AscRelicSuppression : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.AURA,
            BuffAddType = BuffAddType.REPLACE_EXISTING,
        };

        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        Buff Buff;
        float timer = 100.0f;
        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            Buff = buff;
            ApiEventManager.OnDeath.AddListener(this, unit, OnDeath, true);
        }

        public void OnDeath(DeathData deathData)
        {
            Buff.DeactivateBuff();
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
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