using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects.Spell;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using static LeagueSandbox.GameServer.API.ApiMapFunctionManager;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    internal class AscWarp : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.COMBAT_ENCHANCER,
            BuffAddType = BuffAddType.REPLACE_EXISTING,
        };

        public IStatsModifier StatsModifier { get; private set; }

        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            buff.SetStatusEffect(StatusFlags.Stunned, true);
            unit.IconInfo.SwapBorder("Teleport", true, "AscWarp");
            AddParticleTarget(unit, unit, "Global_Asc_teleport", unit, buff.Duration);
        }

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            unit.IconInfo.SwapBorder("", true);
            if (unit is IObjAiBase obj)
            {
                AddBuff("AscWarpReappear", 10.0f, 1, null, unit, obj);
            }
            AddBuff("AscWarpProtection", 2.5f, 1, null, unit, buff.SourceUnit);
        }

        public void OnUpdate(float diff)
        {
        }
    }
}