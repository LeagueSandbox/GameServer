using GameServerCore.Enums;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace HealCheck
{
    internal class HealCheck : IBuffGameScript
    {
        public BuffType BuffType { get; } = BuffType.COMBAT_DEHANCER;
        public BuffAddType BuffAddType { get; } = BuffAddType.REPLACE_EXISTING;
        public int MaxStacks { get; } = 1;
        public bool IsHidden { get; } = false;
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
