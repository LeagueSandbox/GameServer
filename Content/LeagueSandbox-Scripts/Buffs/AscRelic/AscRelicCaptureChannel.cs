using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects;

namespace Buffs
{
    internal class AscRelicCaptureChannel : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.AURA,
            BuffAddType = BuffAddType.REPLACE_EXISTING,
            IsHidden = true
        };

        public StatsModifier StatsModifier { get; private set; }

        Spell Spell;
        AttackableUnit Target;
        ObjAIBase Owner;
        Buff Buff;
        Particle p1;
        Region r1;
        Region r2;
        float windUpTime = 1500.0f;
        bool castWindUp = false;
        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            Buff = buff;
            Spell = ownerSpell;
            Owner = buff.SourceUnit;
            Target = ownerSpell.CastInfo.Targets[0].Unit;
            castWindUp = true;

            p1 = AddParticleTarget(buff.SourceUnit, buff.SourceUnit, "OdinCaptureBeam", Target, 1.5f, 1, "buffbone_glb_channel_loc", "spine", flags: (FXFlags)32);
            r1 = AddUnitPerceptionBubble(unit, 0.0f, buff.Duration, TeamId.TEAM_BLUE, true, unit);
            r2 = AddUnitPerceptionBubble(unit, 0.0f, buff.Duration, TeamId.TEAM_PURPLE, true, unit);
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            RemoveParticle(p1);
            r1.SetToRemove();
            r2.SetToRemove();
        }

        public void OnUpdate(float diff)
        {
            if (castWindUp)
            {
                windUpTime -= diff;
                if (windUpTime <= 0)
                {
                    PlayAnimation(Owner, "Channel", flags: (AnimationFlags)224);
                    AddBuff("AscRelicSuppression", 10.0f, 1, Spell, Target, Owner);
                    AddBuff("OdinChannelVision", 20.0f, 1, Spell, Owner, Owner);

                    p1 = AddParticleTarget(Buff.SourceUnit, Buff.SourceUnit, "OdinCaptureBeamEngaged", Target, Buff.Duration - Buff.TimeElapsed, 1, "BuffBone_Glb_CHANNEL_Loc", "spine");

                    string teamBuff = "OdinBombSuppressionOrder";
                    if (Owner.Team == TeamId.TEAM_PURPLE)
                    {
                        teamBuff = "OdinBombSuppressionChaos";
                    }
                    AddBuff(teamBuff, 10.0f, 1, Spell, Target, Owner);

                    castWindUp = false;
                }
            }
        }
    }
}