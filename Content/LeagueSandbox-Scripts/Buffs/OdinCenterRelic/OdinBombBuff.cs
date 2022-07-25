using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects;
using            GameServerLib.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;

namespace Buffs
{
    internal class OdinBombBuff : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
        };

        public StatsModifier StatsModifier { get; private set; }

        Buff thisBuff;
        Particle blueTeamParticle1;
        Particle blueTeamParticle2;
        Particle blueTeamParticle3;        
        Particle redTeamParticle1;
        Particle redTeamParticle2;
        Particle redTeamParticle3;
        AttackableUnit Unit;

        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
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

            unit.IconInfo.ChangeIcon(iconCategory);
        }
        public void OnDeath(DeathData deathData)
        {
            thisBuff.DeactivateBuff();
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            RemoveParticle(blueTeamParticle1);
            RemoveParticle(blueTeamParticle2);
            RemoveParticle(blueTeamParticle3);
            RemoveParticle(redTeamParticle1);
            RemoveParticle(redTeamParticle2);
            RemoveParticle(redTeamParticle3);
        }
    }
}