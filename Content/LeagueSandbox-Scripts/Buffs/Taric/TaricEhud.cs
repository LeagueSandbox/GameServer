using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace TaricEhud
{
    internal class TaricEhud : IBuffGameScript
    {
        public BuffType BuffType { get; } = BuffType.INTERNAL;
        public BuffAddType BuffAddType { get; } = BuffAddType.REPLACE_EXISTING;
        public int MaxStacks { get; } = 1;
        public bool IsHidden { get; } = true;
        public bool IsUnique { get; } = true;

        public IStatsModifier StatsModifier { get; private set; }

        public void OnActivate(IObjAiBase unit, IBuff buff, ISpell ownerSpell)
        {
        }

        public void OnDeactivate(IObjAiBase unit)
        {

        }

        public void OnUpdate(double diff)
        {

        }
    }
}
