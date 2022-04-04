using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects.Spell;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using static LeagueSandbox.GameServer.API.ApiMapFunctionManager;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    internal class AscRespawn : IBuffGameScript
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
                if(obj is IChampion)
                {
                    NotifyMapAnnouncement(EventID.OnChampionAscended, sourceNetId: obj.NetId);
                }

                NotifyAscendant(obj);
                SetStatus(unit, StatusFlags.Targetable, false);
                unit.PauseAnimation(true);

                AddParticleTarget(unit, unit, "EggTimer", unit, buff.Duration, flags: (FXFlags)32);
                AddParticle(unit, unit, "AscTransferGlow", unit.Position, buff.Duration, flags: (FXFlags)32);
                AddParticle(unit, unit, "AscTurnToStone", unit.Position, buff.Duration, flags: (FXFlags)32);
            }
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            unit.PauseAnimation(false);
            if(unit is IObjAiBase obj)
            {
                AddBuff("AscBuffIcon", 25000.0f, 1, null, unit, obj);
                if(unit is IMonster)
                {
                    AddBuff("AscXerathControl", 999999.0f, 1, null, unit, obj);
                }
            }
            SetStatus(unit, StatusFlags.Targetable, true);
        }

        public void OnUpdate(float diff)
        {
            soundTimer -= diff;
            if(soundTimer <= 0 && !hasNotifiedSound)
            {
                PlaySound("Play_sfx_ZhonyasRingShield_OnBuffActivate", Unit);
                PlaySound("Play_sfx_Cassiopeia_CassiopeiaPetrifyingGazeStun_OnBuffActivate", Unit);
                hasNotifiedSound = true;
            }
        }
    }
}