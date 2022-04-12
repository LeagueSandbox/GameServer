using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects.Spell;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    internal class AscRelicCaptureChannel : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.AURA,
            BuffAddType = BuffAddType.REPLACE_EXISTING,
            IsHidden = true
        };

        public IStatsModifier StatsModifier { get; private set; }

        ISpell Spell;
        IAttackableUnit Target;
        IObjAiBase Owner;
        IBuff Buff;
        IParticle p1;
        IRegion r1;
        IRegion r2;
        float windUpTime;
        bool castWindUp = false;
        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            Buff = buff;
            Spell = ownerSpell;
            Owner = buff.SourceUnit;
            Target = ownerSpell.CastInfo.Targets[0].Unit;

            p1 = AddParticleTarget(buff.SourceUnit, buff.SourceUnit, "OdinCaptureBeam", Target, 1.5f, 1, "buffbone_glb_channel_loc", "spine", flags: (FXFlags)32);
            windUpTime = 1500.0f;
            castWindUp = true;
            r1 = AddUnitPerceptionBubble(unit, 0.0f, buff.Duration, TeamId.TEAM_BLUE, true, unit);
            r2 = AddUnitPerceptionBubble(unit, 0.0f, buff.Duration, TeamId.TEAM_PURPLE, true, unit);
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
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