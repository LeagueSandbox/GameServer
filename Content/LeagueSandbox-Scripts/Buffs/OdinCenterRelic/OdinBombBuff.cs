using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects.Spell;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using static LeagueSandbox.GameServer.API.ApiMapFunctionManager;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    internal class OdinBombBuff : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
        };

        public IStatsModifier StatsModifier { get; private set; }

        IBuff thisBuff;
        IParticle blueTeamParticle1;
        IParticle blueTeamParticle2;
        IParticle blueTeamParticle3;        
        IParticle redTeamParticle1;
        IParticle redTeamParticle2;
        IParticle redTeamParticle3;
        IAttackableUnit Unit;

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            Unit = unit;
            thisBuff = buff;

            buff.SetStatusEffect(StatusFlags.CanMove, false);
            buff.SetStatusEffect(StatusFlags.ForceRenderParticles, true);
            buff.SetStatusEffect(StatusFlags.Targetable, false);

            ApiEventManager.OnDeath.AddListener(this, unit, OnDeath, true);

            if (unit.Team == TeamId.TEAM_BLUE)
            {
                redTeamParticle1=AddParticleTarget(Unit, null, "Odin_Prism_Green", Unit, buff.Duration, teamOnly: TeamId.TEAM_PURPLE);
                redTeamParticle2=AddParticleTarget(Unit, null, "Odin_Prism_Ground_Green", Unit, buff.Duration, teamOnly: TeamId.TEAM_PURPLE);
                redTeamParticle3=AddParticleTarget(Unit, null, "odin_relic_buf_green", Unit, buff.Duration, teamOnly: TeamId.TEAM_PURPLE);
                blueTeamParticle1=AddParticleTarget(Unit, null, "Odin_Prism_Red", Unit, buff.Duration, teamOnly: TeamId.TEAM_BLUE);
                blueTeamParticle2=AddParticleTarget(Unit, null, "Odin_Prism_Ground_Red", Unit, buff.Duration, teamOnly: TeamId.TEAM_BLUE);
                blueTeamParticle3=AddParticleTarget(Unit, null, "odin_relic_buf_red", Unit, buff.Duration, teamOnly: TeamId.TEAM_BLUE);
            }
            else
            {
                blueTeamParticle1=AddParticleTarget(Unit, null, "Odin_Prism_Green", Unit, buff.Duration, teamOnly: TeamId.TEAM_BLUE);
                blueTeamParticle2=AddParticleTarget(Unit, null, "Odin_Prism_Ground_Green", Unit, buff.Duration, teamOnly: TeamId.TEAM_BLUE);
                blueTeamParticle3=AddParticleTarget(Unit, null, "odin_relic_buf_green", Unit, buff.Duration, teamOnly: TeamId.TEAM_BLUE);
                redTeamParticle1=AddParticleTarget(Unit, null, "Odin_Prism_Red", Unit, buff.Duration, teamOnly: TeamId.TEAM_PURPLE);
                redTeamParticle2=AddParticleTarget(Unit, null, "Odin_Prism_Ground_Red", Unit, buff.Duration, teamOnly: TeamId.TEAM_PURPLE);
                redTeamParticle3=AddParticleTarget(Unit, null, "odin_relic_buf_red", Unit, buff.Duration, teamOnly: TeamId.TEAM_PURPLE);
            }

            string iconCategory = "CenterRelicLeft";

            if (unit.Team == TeamId.TEAM_PURPLE)
            {
                iconCategory = "CenterRelicRight";
            }

            SetMinimapIcon(unit, iconCategory, true);
        }
        public void OnDeath(IDeathData deathData)
        {
            thisBuff.DeactivateBuff();
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            RemoveParticle(blueTeamParticle1);
            RemoveParticle(blueTeamParticle2);
            RemoveParticle(blueTeamParticle3);
            RemoveParticle(redTeamParticle1);
            RemoveParticle(redTeamParticle2);
            RemoveParticle(redTeamParticle3);
        }

        public void OnUpdate(float diff)
        {
        }
    }
}