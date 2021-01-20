using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.Spells;

namespace LeagueSandbox.GameServer.Scripting.CSharp
{
    public interface IBuffGameScript
    {
        BuffType BuffType { get; }
        BuffAddType BuffAddType { get; }
        bool IsHidden { get; }
        int MaxStacks { get; }
        IStatsModifier StatsModifier { get; }

        void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell);
        void OnDeactivate(IAttackableUnit unit);
        void OnUpdate(double diff);
    }
}
