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
    internal class AscBuff : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.INTERNAL,
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };
        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        IParticle p1;
        IParticle p2;
        IBuff thisBuff;
        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            thisBuff = buff;
            if (unit is IObjAiBase obj)
            {
                AddBuff("AscBuffIcon", 25000.0f, 1, null, unit, obj);
            }

            PlaySound("Stop_sfx_ZhonyasRingShield_OnBuffActivate", unit);
            PlaySound("Play_sfx_Leona_LeonaSolarFlare_hit", unit);
            PlaySound("Play_sfx_Lux_LuxIlluminationPassive_hit", unit);

            p1 = AddParticle(unit, unit, "Global_Asc_Avatar", unit.Position, -1);
            p2 = AddParticle(unit, unit, "Global_Asc_Aura", unit.Position, -1);
            AddParticleTarget(unit, unit, "AscForceBubble", unit, size: 3.3f);

            ApiEventManager.OnDeath.AddListener(unit, unit, OnDeath, true);

            //Unit Perception bubbles seems to be broken
            AddUnitPerceptionBubble(unit, 0.0f, 25000f, TeamId.TEAM_BLUE, true, unit);
            AddUnitPerceptionBubble(unit, 0.0f, 25000f, TeamId.TEAM_PURPLE, true, unit);

            //Note: The ascension applies a "MoveAway" knockback debuff on all enemies around.
            //The duration varies based on the distance (yet unknown) you were to whoever ascended. And the PathSpeedOverride and ParabolicGravity varies based on the duration.
            //PathSpeedOverride and ParabolicGravity with 0.5 duration: Speed - 1200 / ParabolicGravity - 10.0
            //PathSpeedOverride and ParabolicGravity with 0.75 duration: Speed - 1600 / ParabolicGravity - 7.0
        }

        public void OnDeath(IDeathData deathData)
        {
            if (deathData.Unit is IMonster xerath)
            {
                AddBuff("AscBuffTransfer", 5.7f, 1, null, deathData.Killer, xerath);
            }
            else if (deathData.Unit is IChampion)
            {
                NotifyAscendant();
                NotifyWorldEvent(EventID.OnStartGameMessage3, 8);
                NotifyWorldEvent(EventID.OnClearAscended);
            }

            deathData.Unit.GetBuffWithName("AscBuffIcon").DeactivateBuff();
            thisBuff.DeactivateBuff();
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            RemoveParticle(p1);
            RemoveParticle(p2);
            unit.PauseAnimation(false);
        }

        public void OnUpdate(float diff)
        {
        }
    }
}