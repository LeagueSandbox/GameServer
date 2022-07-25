using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Scripting.CSharp;
using System.Linq;
using System.Numerics;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;

namespace Buffs
{
    internal class OdinSpeedShrineAura : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
        };

        public StatsModifier StatsModifier { get; private set; }

        Particle buffParticle;
        AttackableUnit Unit;
        float timer = 250f;
        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            Unit = unit;

            buff.SetStatusEffect(StatusFlags.Targetable, false);
            buff.SetStatusEffect(StatusFlags.Invulnerable, true);
            buff.SetStatusEffect(StatusFlags.ForceRenderParticles, true);
            buff.SetStatusEffect(StatusFlags.NoRender, true);

            buffParticle = AddParticleTarget(Unit, null, "Odin_Shrine_Time", Unit, buff.Duration);
        }

        public void OnUpdate(float diff)
        {
            timer += diff;
            if (Unit != null && timer >= 100)
            {
                var units = GetUnitsInRange(Unit.Position, 350f, true).OrderBy(unit => Vector2.DistanceSquared(unit.Position, Unit.Position)).ToList();
                units.RemoveAll(x => !(x is Champion));
                if (units.Count >= 1)
                {
                    AddBuff("OdinSpeedShrineBuff", 10.0f, 1, null, units[0], null);
                }
                timer = 0;
            }
        }
    }
}