using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects.Spell;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using static LeagueSandbox.GameServer.API.ApiMapFunctionManager;
using LeagueSandbox.GameServer.API;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.GameObjects.Stats;

namespace Buffs
{
    internal class AscBuffIcon : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER,
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };
        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        IParticle p1;
        IParticle p2;
        IBuff thisBuff;
        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            thisBuff = buff;

            //Note: The ascension applies a knockback debuff on all enemies around. The exact areis still unkown
            //AddBuff("MoveAway", 0.75f, 1, null, units in area, unit as IChampion);

            PlaySound("Stop_sfx_ZhonyasRingShield_OnBuffActivate", unit);
            PlaySound("Play_sfx_Leona_LeonaSolarFlare_hit", unit);
            PlaySound("Play_sfx_Lux_LuxIlluminationPassive_hit", unit);
            SetBuffToolTipVar(buff, 3, 15.0f);

            p1 = AddParticle(unit, unit, "Global_Asc_Avatar", unit.Position, -1);
            p2 = AddParticle(unit, unit, "Global_Asc_Aura", unit.Position, -1);
            AddParticleTarget(unit, unit, "AscForceBubble", unit, size: 3.3f);
            AddParticleTarget(unit, unit, "CassPetrifyMiss_tar", unit, size: 3.0f);
            AddParticleTarget(unit, unit, "Rebirth_cas", unit);
            AddParticleTarget(unit, unit, "TurnBack", unit);
            AddParticleTarget(unit, unit, "LeonaPassive_tar", unit, size: 2.5f);

            ApiEventManager.OnDeath.AddListener(unit, unit, OnDeath, true);

            StatsModifier.HealthPoints.FlatBonus = 150.0f;
            StatsModifier.AttackDamage.FlatBonus = 36.0f;
            StatsModifier.AbilityPower.FlatBonus = 36.0f;
            StatsModifier.ArmorPenetration.PercentBonus = 0.15f;
            StatsModifier.MagicPenetration.PercentBonus = 0.15f;
            StatsModifier.CooldownReduction.FlatBonus = 0.25f;
            StatsModifier.Size.FlatBonus = 2;

            unit.AddStatModifier(StatsModifier);

            //TODO: 
            // - Add 100% mana/energy cost reduction
            // - Add 50% Health cost reduction
            AddUnitPerceptionBubble(unit, 0f, 25000f, TeamId.TEAM_BLUE, true, unit);
            AddUnitPerceptionBubble(unit, 0f, 25000f, TeamId.TEAM_PURPLE, true, unit);
        }

        public void OnDeath(IDeathData deathData)
        {
            thisBuff.DeactivateBuff();
            if (deathData.Unit is IMonster xerath)
            {
                AddBuff("AscRespawn", 5.7f, 1, null, deathData.Killer, xerath);
            }
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            unit.PauseAnimation(false);
        }

        public void OnUpdate(float diff)
        {
        }
    }
}