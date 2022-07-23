using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using static LeagueSandbox.GameServer.API.ApiMapFunctionManager;
using static LeagueSandbox.GameServer.API.ApiGameEvents;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;

namespace Buffs
{
    internal class AscBuffTransfer : IBuffGameScript
    {
        public BuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.INTERNAL,
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };
        public StatsModifier StatsModifier { get; private set; }

        float soundTimer = 1000.0f;
        AttackableUnit Unit;
        bool hasNotifiedSound = false;
        public void OnActivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            Unit = unit;

            if (unit is ObjAIBase obj)
            {
                if (unit is Champion ch)
                {
                    AnnounceChampionAscended(ch);
                }
                else if (unit is Monster mo)
                {
                    AnnounceMinionAscended(mo);
                }
                NotifyAscendant(obj);
            }

            unit.PauseAnimation(true);

            AddParticleTarget(unit, unit, "EggTimer", unit, buff.Duration, flags: (FXFlags)32);
            AddParticle(unit, unit, "AscTransferGlow", unit.Position, buff.Duration, flags: (FXFlags)32);
            AddParticle(unit, unit, "AscTurnToStone", unit.Position, buff.Duration, flags: (FXFlags)32);

            buff.SetStatusEffect(StatusFlags.Targetable, false);
            buff.SetStatusEffect(StatusFlags.Stunned, true);
            buff.SetStatusEffect(StatusFlags.Invulnerable, true);
        }

        public void OnDeactivate(AttackableUnit unit, Buff buff, Spell ownerSpell)
        {
            unit.PauseAnimation(false);

            AddParticleTarget(unit, unit, "CassPetrifyMiss_tar", unit, size: 3.0f);
            AddParticleTarget(unit, unit, "Rebirth_cas", unit);
            AddParticleTarget(unit, unit, "TurnBack", unit);
            AddParticleTarget(unit, unit, "LeonaPassive_tar", unit, size: 2.5f);

            if (unit is ObjAIBase obj)
            {
                AddBuff("AscBuff", 25000.0f, 1, null, unit, obj);
            }
        }

        public void OnUpdate(float diff)
        {
            soundTimer -= diff;
            if (soundTimer <= 0 && !hasNotifiedSound)
            {
                PlaySound("Play_sfx_ZhonyasRingShield_OnBuffActivate", Unit);
                PlaySound("Play_sfx_Cassiopeia_CassiopeiaPetrifyingGazeStun_OnBuffActivate", Unit);
                hasNotifiedSound = true;
            }
        }
    }
}