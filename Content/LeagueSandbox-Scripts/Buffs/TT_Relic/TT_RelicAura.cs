using System.Numerics;
using System.Linq;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects.Spell;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    internal class TT_RelicAura : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {

        };
        public IStatsModifier StatsModifier { get; private set; }

        bool setToKill;
        IBuff thisBuff;
        IParticle buffParticle;
        IParticle buffParticle2;
        IAttackableUnit Unit;
        float timer = 250f;
        IMinion InvisibleMinion;
        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            thisBuff = buff;
            Unit = unit;

            buff.SetStatusEffect(StatusFlags.Targetable, false);
            buff.SetStatusEffect(StatusFlags.Invulnerable, true);
            buff.SetStatusEffect(StatusFlags.ForceRenderParticles, true);
            buff.SetStatusEffect(StatusFlags.NoRender, true);

            InvisibleMinion = AddMinion(null, "TestCubeRender", "HiddenMinion", unit.Position, ignoreCollision: true);

            if (unit is IObjAiBase obj)
            {
                AddBuff("ResistantSkinDragon", 25000f, 1, null, InvisibleMinion, obj, false);
            }

            buffParticle = AddParticleTarget(unit, unit, "TT_Heal_Rune", unit, -1f);
            buffParticle2 = AddParticleTarget(unit, unit, "TT_Heal_RuneWell", unit, -1f);

            setToKill = false;
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            buffParticle.SetToRemove();
            buffParticle2.SetToRemove();

            SetStatus(unit, StatusFlags.Targetable, true);
            SetStatus(unit, StatusFlags.Invulnerable, false);

            unit.TakeDamage(unit, 250000.0f, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_INTERNALRAW, false);
            InvisibleMinion.TakeDamage(unit, 250000.0f, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_INTERNALRAW, false);

            SetStatus(unit, StatusFlags.NoRender, true);
        }

        public void OnUpdate(float diff)
        {
            if (setToKill)
            {
                thisBuff.DeactivateBuff();
            }
            timer += diff;
            if (Unit != null && timer >= 250)
            {
                var units = GetUnitsInRange(Unit.Position, 175f, true).OrderBy(unit => Vector2.DistanceSquared(unit.Position, Unit.Position)).ToList();
                units.RemoveAll(x => !(x is IChampion));
                if (units.Count >= 1)
                {
                    if (!setToKill)
                    {
                        AddParticle(Unit, null, "TT_Heal_RuneCapture", Unit.Position);
                        AddBuff("TT_RelicHeal", 0.1f, 1, null, units[0], null);
                        AddBuff("TT_SpeedShrine_Buff", 5, 1, null, units[0], null);

                        setToKill = true;
                    }
                }
                timer = 0;
            }
        }
    }
}