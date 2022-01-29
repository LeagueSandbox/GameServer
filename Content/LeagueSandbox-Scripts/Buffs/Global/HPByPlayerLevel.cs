using System.Linq;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace Buffs
{
    internal class HPByPlayerLevel : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        IObjAiBase owner;
        int maxPlayerLevel;
        float tickTime;

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            var owner = unit as IObjAiBase;
            foreach (IAttackableUnit target in GetUnitsInRange(unit.Position, 9999.0f, true).Where(x => x is IChampion))
            {
                // TODO: Use a global "MaxPlayerLevel" variable.
                if (target.Stats.Level > maxPlayerLevel)
                {
                    maxPlayerLevel = target.Stats.Level;
                }
            }
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
        }

        public void OnUpdate(float diff)
        {
            // Executed every 10 seconds.
            if (owner != null)
            {
                if (tickTime > 10000.0f)
                {
                    var healthPercent = (owner.Stats.HealthPoints.Total - owner.Stats.CurrentHealth) / owner.Stats.HealthPoints.Total;
                    if (healthPercent >= 0.99f)
                    {
                        foreach (IAttackableUnit target in GetUnitsInRange(owner.Position, 9999.0f, true).Where(x => x is IChampion))
                        {
                            // TODO: Use a global "MaxPlayerLevel" variable.
                            if (target.Stats.Level > maxPlayerLevel)
                            {
                                maxPlayerLevel = target.Stats.Level;
                            }
                        }
                    }

                    tickTime = 0;
                }

                // TODO: Find out how OnUpdateStats is supposed to work so we can do this without it being spammed every update tick.
                // StatsModifier.HealthPoints.FlatBonus = owner.CharData.HpPerLevel * maxPlayerLevel;
                // owner.AddStatModifier(StatsModifier);

                tickTime += diff;
            }
        }
    }
}
