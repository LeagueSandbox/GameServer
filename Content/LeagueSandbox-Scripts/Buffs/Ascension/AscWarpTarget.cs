using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects.Spell;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using static LeagueSandbox.GameServer.API.ApiMapFunctionManager;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;

namespace Buffs
{
    internal class AscWarpTarget : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER,
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };

        public IStatsModifier StatsModifier { get; private set; }

        IParticle p1;
        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            //For some still unknown reson, the borders aren't getting applied to the minion, but if this buff gets applied to the teleport plates, it works(?)
            SetMinimapIcon(unit, changeBorder: true, borderCategory: "Teleport", borderScriptName: "ascwarptarget");
            SetMinimapIcon(unit, changeBorder: true, borderCategory: "Teleport", borderScriptName: "ascwarptarget");
            SetMinimapIcon(unit, changeBorder: true, borderCategory: "Teleport", borderScriptName: "ascwarptarget");
            SetMinimapIcon(unit, "NoIcon", true);
            p1 = AddParticleTarget(buff.SourceUnit, null, "global_asc_teleport_target", unit, -1);
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            RemoveParticle(p1);
            SetMinimapIcon(unit, changeBorder: true);
            TeleportTo(buff.SourceUnit, unit.Position.X, unit.Position.Y);
            if(buff.SourceUnit is IChampion ch)
            {
                TeleportCamera(ch, new Vector3(unit.Position.X, unit.GetHeight(), unit.Position.Y));
            }
            unit.Die(CreateDeathData(false, 3, unit, unit, DamageType.DAMAGE_TYPE_PHYSICAL, DamageSource.DAMAGE_SOURCE_RAW, 0.0f));
        }

        public void OnUpdate(float diff)
        {
        }
    }
}