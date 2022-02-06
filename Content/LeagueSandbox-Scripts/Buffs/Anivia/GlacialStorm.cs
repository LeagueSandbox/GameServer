using System.Numerics;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace Buffs
{
    class GlacialStorm : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER,
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        IAttackableUnit owner;
        ISpell originSpell;
        IBuff thisBuff;
        IParticle red;
        IParticle green;
        float DamageManaTimer;
        float SlowTimer;
        float[] manaCost = { 40.0f, 50.0f, 60.0f };

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            owner = unit;
            originSpell = ownerSpell;
            thisBuff = buff;

            var spellPos = new Vector2(originSpell.CastInfo.TargetPositionEnd.X, originSpell.CastInfo.TargetPositionEnd.Z);

            originSpell.SetCooldown(1.0f, true);

            SetTargetingType((IObjAiBase)unit, SpellSlotType.SpellSlots, 3, TargetingType.Self);

            if (owner.Team == TeamId.TEAM_BLUE)
            {
                red = AddParticle(owner, null, "cryo_storm_red_team", spellPos, lifetime: buff.Duration, teamOnly: TeamId.TEAM_PURPLE);
                green = AddParticle(owner, null, "cryo_storm_green_team", spellPos, lifetime: buff.Duration, teamOnly: TeamId.TEAM_BLUE);
            }
            else
            {
                red = AddParticle(owner, null, "cryo_storm_red_team", spellPos, lifetime: buff.Duration, teamOnly: TeamId.TEAM_BLUE);
                green = AddParticle(owner, null, "cryo_storm_green_team", spellPos, lifetime: buff.Duration, teamOnly: TeamId.TEAM_PURPLE);
            }
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            ownerSpell.SetCooldown(6.0f);

            SetTargetingType((IObjAiBase)unit, SpellSlotType.SpellSlots, 3, TargetingType.Area);

            RemoveParticle(red);
            RemoveParticle(green);

            if (ownerSpell.Script is Spells.GlacialStorm spellScript)
            {
                spellScript.DamageSector.ExecuteTick();
                spellScript.DamageSector.SetToRemove();
                spellScript.SlowSector.ExecuteTick();
                spellScript.SlowSector.SetToRemove();
            }
        }

        public void OnUpdate(float diff)
        {
            if (owner != null && thisBuff != null && originSpell != null)
            {
                DamageManaTimer += diff;

                if (DamageManaTimer >= 500f)
                {
                    if (manaCost[originSpell.CastInfo.SpellLevel - 1] > owner.Stats.CurrentMana)
                    {
                        RemoveBuff(thisBuff);
                    }
                    else
                    {
                        owner.Stats.CurrentMana -= manaCost[originSpell.CastInfo.SpellLevel - 1];
                    }

                    DamageManaTimer = 0;
                }

                SlowTimer += diff;

                if (SlowTimer >= 250f)
                {
                    var spellPos = new Vector2(originSpell.CastInfo.TargetPositionEnd.X, originSpell.CastInfo.TargetPositionEnd.Z);
                    if ((owner is IObjAiBase ai && !ai.CanCast()) || !GameServerCore.Extensions.IsVectorWithinRange(owner.Position, spellPos, 1200f))
                    {
                        RemoveBuff(thisBuff);
                    }
                }
            }
        }
    }
}
