using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects.Spell;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using static LeagueSandbox.GameServer.API.ApiMapFunctionManager;
using static LeagueSandbox.GameServer.API.ApiGameEvents;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    internal class AscBuffTransfer : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.INTERNAL,
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };
        public IStatsModifier StatsModifier { get; private set; }

        float soundTimer = 1000.0f;
        IAttackableUnit Unit;
        bool hasNotifiedSound = false;
        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            Unit = unit;

            if (unit is IObjAiBase obj)
            {
                if (unit is IChampion ch)
                {
                    AnnounceChampionAscended(ch);

                }
                else if (unit is IMonster mo)
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

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            unit.PauseAnimation(false);

            AddParticleTarget(unit, unit, "CassPetrifyMiss_tar", unit, size: 3.0f);
            AddParticleTarget(unit, unit, "Rebirth_cas", unit);
            AddParticleTarget(unit, unit, "TurnBack", unit);
            AddParticleTarget(unit, unit, "LeonaPassive_tar", unit, size: 2.5f);

            if (unit is IObjAiBase obj)
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