using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects;
using            GameServerLib.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using static LeagueSandbox.GameServer.API.ApiEventManager;

namespace Buffs
{
    internal class MordekaiserChildrenOfTheGrave : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.DAMAGE,
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        Buff Buff;
        float timer = 1000.0f;
        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            Buff = buff;
            OnDeath.AddListener(this, unit, OnTargetDeath, true);

            float basePercentDamage = 0.12f + (0.025f * (ownerSpell.CastInfo.SpellLevel - 1));
            float AP = buff.SourceUnit.Stats.AbilityPower.Total * 0.0002f;
            float damage = buff.TargetUnit.Stats.HealthPoints.Total * (basePercentDamage + AP);

            var data = buff.TargetUnit.TakeDamage(buff.SourceUnit, damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_PROC, false);
            buff.SourceUnit.Stats.CurrentHealth += data.PostMitigationDamage;
        }

        public void OnTargetDeath(DeathData data)
        {
            AddBuff("MordekaiserCOTGRevive", 30.0f, 1, Buff.OriginSpell, data.Unit, Buff.SourceUnit);
            RemoveBuff(Buff);
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            OnDeath.RemoveListener(this);
        }

        public void OnUpdate(float diff)
        {
            timer -= diff;

            if (timer <= 0)
            {
                AddBuff("MordekaiserCOTGDot", 0.01f, 1, Buff.OriginSpell, Buff.TargetUnit, Buff.SourceUnit);
                timer = 1000.0f;
            }
        }
    }
}
