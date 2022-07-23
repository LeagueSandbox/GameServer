using System.Linq;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace Buffs
{
    internal class HPByPlayerLevel : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        ObjAIBase owner;
        int maxPlayerLevel;
        float tickTime;

        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            var owner = unit as ObjAIBase;
            foreach (AttackableUnit target in GetUnitsInRange(unit.Position, 9999.0f, true).Where(x => x is Champion))
            {
                // TODO: Use a global "MaxPlayerLevel" variable.
                if (target.Stats.Level > maxPlayerLevel)
                {
                    maxPlayerLevel = target.Stats.Level;
                }
            }
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
                        foreach (AttackableUnit target in GetUnitsInRange(owner.Position, 9999.0f, true).Where(x => x is Champion))
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
