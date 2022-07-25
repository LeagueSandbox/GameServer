using System.Numerics;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace Buffs
{
    class GlacialStorm : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER,
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();

        AttackableUnit owner;
        Spell originSpell;
        Buff thisBuff;
        Particle red;
        Particle green;
        float DamageManaTimer;
        float SlowTimer;
        float[] manaCost = { 40.0f, 50.0f, 60.0f };

        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            owner = unit;
            originSpell = ownerSpell;
            thisBuff = buff;

            var spellPos = new Vector2(originSpell.CastInfo.TargetPositionEnd.X, originSpell.CastInfo.TargetPositionEnd.Z);

            originSpell.SetCooldown(1.0f, true);

            SetTargetingType((ObjAIBase)unit, SpellSlotType.SpellSlots, 3, TargetingType.Self);

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

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            ownerSpell.SetCooldown(6.0f);

            SetTargetingType((ObjAIBase)unit, SpellSlotType.SpellSlots, 3, TargetingType.Area);

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
                    if ((owner is ObjAIBase ai && !ai.CanCast(originSpell)) || !GameServerCore.Extensions.IsVectorWithinRange(owner.Position, spellPos, 1200f))
                    {
                        RemoveBuff(thisBuff);
                    }
                }
            }
        }
    }
}
