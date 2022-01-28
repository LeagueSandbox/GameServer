﻿using GameServerCore.Domain.GameObjects;
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
using GameServerCore.Domain;

namespace Buffs
{
    internal class OdinBombBuff : IBuffGameScript
    {
        public BuffType BuffType => BuffType.INTERNAL;
        public BuffAddType BuffAddType => BuffAddType.RENEW_EXISTING;
        public int MaxStacks => 1;
        public bool IsHidden => false;

        public IStatsModifier StatsModifier { get; private set; }

        bool hasNotified = false;
        IBuff thisBuff;
        IParticle blueTeamParticle1;
        IParticle blueTeamParticle2;
        IParticle blueTeamParticle3;        
        IParticle redTeamParticle1;
        IParticle redTeamParticle2;
        IParticle redTeamParticle3;
        IAttackableUnit Unit;
        float timer = 250f;
        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            Unit = unit;
            thisBuff = buff;
            SetStatus(unit, StatusFlags.CanMove, false);
            SetStatus(unit, StatusFlags.ForceRenderParticles, true);
            SetStatus(unit, StatusFlags.Targetable, false);
            ApiEventManager.OnDeath.AddListener(this, unit, OnDeath, true);
            if (unit.Team == TeamId.TEAM_BLUE)
            {
                redTeamParticle1=AddParticleTarget(Unit, null, "Odin_Prism_Green", Unit, buff.Duration, reqVision: false, teamOnly: TeamId.TEAM_PURPLE);
                redTeamParticle2=AddParticleTarget(Unit, null, "Odin_Prism_Ground_Green", Unit, buff.Duration, reqVision: false, teamOnly: TeamId.TEAM_PURPLE);
                redTeamParticle3=AddParticleTarget(Unit, null, "odin_relic_buf_green", Unit, buff.Duration, reqVision: false, teamOnly: TeamId.TEAM_PURPLE);
                blueTeamParticle1=AddParticleTarget(Unit, null, "Odin_Prism_Red", Unit, buff.Duration, reqVision: false, teamOnly: TeamId.TEAM_BLUE);
                blueTeamParticle2=AddParticleTarget(Unit, null, "Odin_Prism_Ground_Red", Unit, buff.Duration, reqVision: false, teamOnly: TeamId.TEAM_BLUE);
                blueTeamParticle3=AddParticleTarget(Unit, null, "odin_relic_buf_red", Unit, buff.Duration, reqVision: false, teamOnly: TeamId.TEAM_BLUE);
            }
            else
            {
                blueTeamParticle1=AddParticleTarget(Unit, null, "Odin_Prism_Green", Unit, buff.Duration, reqVision: false, teamOnly: TeamId.TEAM_BLUE);
                blueTeamParticle2=AddParticleTarget(Unit, null, "Odin_Prism_Ground_Green", Unit, buff.Duration, reqVision: false, teamOnly: TeamId.TEAM_BLUE);
                blueTeamParticle3=AddParticleTarget(Unit, null, "odin_relic_buf_green", Unit, buff.Duration, reqVision: false, teamOnly: TeamId.TEAM_BLUE);
                redTeamParticle1=AddParticleTarget(Unit, null, "Odin_Prism_Red", Unit, buff.Duration, reqVision: false, teamOnly: TeamId.TEAM_PURPLE);
                redTeamParticle2=AddParticleTarget(Unit, null, "Odin_Prism_Ground_Red", Unit, buff.Duration, reqVision: false, teamOnly: TeamId.TEAM_PURPLE);
                redTeamParticle3=AddParticleTarget(Unit, null, "odin_relic_buf_red", Unit, buff.Duration, reqVision: false, teamOnly: TeamId.TEAM_PURPLE);
            }
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
            if(!hasNotified && Unit != null)
            {
                string iconCategory = "CenterRelicLeft";

                if (Unit.Team == TeamId.TEAM_PURPLE)
                {
                    iconCategory = "CenterRelicRight";
                }
                //For some Reason this only works here
                NotifyUnitMinimapIconUpdate(Unit, iconCategory, true);
                hasNotified = true;
            }
        }
    }
}