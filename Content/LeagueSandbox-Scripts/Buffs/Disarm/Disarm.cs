using LeagueSandbox.GameServer.API;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Disarm
{
    internal class Disarm : IBuffGameScript
    {
        public BuffType BuffType { get; } = BuffType.DISARM;
        public BuffAddType BuffAddType { get; } = BuffAddType.REPLACE_EXISTING;
        public int MaxStacks { get; } = 1;
        public bool IsHidden { get; } = false;
        public bool IsUnique { get; } = false;

        public IStatsModifier StatsModifier { get; private set; }

        private UnitCrowdControl _crowd = new UnitCrowdControl(CrowdControlType.DISARM);

        public void OnActivate(IObjAiBase unit, IBuff buff, ISpell ownerSpell)
        {
            unit.ApplyCrowdControl(_crowd);
        }

        public void OnDeactivate(IObjAiBase unit)
        {
            unit.RemoveCrowdControl(_crowd);
        }

        public void OnUpdate(double diff)
        {

        }
    }
}

