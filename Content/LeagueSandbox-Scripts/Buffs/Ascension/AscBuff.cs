using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using static LeagueSandbox.GameServer.API.ApiGameEvents;
using LeagueSandbox.GameServer.API;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using GameServerLib.GameObjects.AttackableUnits;

namespace Buffs
{
    internal class AscBuff : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.INTERNAL,
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };
        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        Particle p1;
        Particle p2;
        Buff thisBuff;
        Region r1;
        Region r2;
        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            thisBuff = buff;
            if (unit is ObjAIBase obj)
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
            r1 = AddUnitPerceptionBubble(unit, 0.0f, 25000f, TeamId.TEAM_BLUE, true, unit);
            r2 = AddUnitPerceptionBubble(unit, 0.0f, 25000f, TeamId.TEAM_PURPLE, true, unit);

            //Note: The ascension applies a "MoveAway" knockback debuff on all enemies around.
            //The duration varies based on the distance (yet unknown) you were to whoever ascended. And the PathSpeedOverride and ParabolicGravity varies based on the duration.
            //PathSpeedOverride and ParabolicGravity with 0.5 duration: Speed - 1200 / ParabolicGravity - 10.0
            //PathSpeedOverride and ParabolicGravity with 0.75 duration: Speed - 1600 / ParabolicGravity - 7.0
        }

        public void OnDeath(DeathData deathData)
        {
            if (deathData.Unit is Monster xerath)
            {
                AddBuff("AscBuffTransfer", 5.7f, 1, null, deathData.Killer, xerath);
            }
            else if (deathData.Unit is Champion)
            {
                AnnounceStartGameMessage(3, 8);
                AnnounceClearAscended();
            }

            deathData.Unit.GetBuffWithName("AscBuffIcon").DeactivateBuff();
            thisBuff.DeactivateBuff();
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            RemoveParticle(p1);
            RemoveParticle(p2);
            r1.SetToRemove();
            r2.SetToRemove();
            unit.PauseAnimation(false);
        }
    }
}