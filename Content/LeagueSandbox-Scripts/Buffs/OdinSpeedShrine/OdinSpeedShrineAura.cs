using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects.Spell;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using GameServerCore.Domain.GameObjects.Spell.Sector;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using System.Linq;
using System.Numerics;

namespace Buffs
{
    internal class OdinSpeedShrineAura : IBuffGameScript
    {
        public BuffType BuffType => BuffType.INTERNAL;
        public BuffAddType BuffAddType => BuffAddType.RENEW_EXISTING;
        public int MaxStacks => 1;
        public bool IsHidden => false;

        public IStatsModifier StatsModifier { get; private set; }

        IParticle buffParticle;
        IAttackableUnit Unit;
        float timer = 250f;
        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            Unit = unit;

            buff.SetStatusEffect(StatusFlags.Targetable, false);
            buff.SetStatusEffect(StatusFlags.Invulnerable, true);
            buff.SetStatusEffect(StatusFlags.ForceRenderParticles, true);
            buff.SetStatusEffect(StatusFlags.NoRender, true);

            buffParticle = AddParticleTarget(Unit, null, "Odin_Shrine_Time", Unit, buff.Duration, reqVision: false);
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
        }

        public void OnUpdate(float diff)
        {
            timer += diff;
            if (Unit != null && timer >= 100)
            {
                var units = GetUnitsInRange(Unit.Position, 350f, true).OrderBy(unit => Vector2.DistanceSquared(unit.Position, Unit.Position)).ToList();
                units.RemoveAll(x => !(x is IChampion));
                if (units.Count >= 1)
                {
                    AddBuff("OdinSpeedShrineBuff", 10.0f, 1, null, units[0], null);
                }
                timer = 0;
            }
        }
    }
}