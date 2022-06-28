using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;

namespace GameServerCore.Scripting.CSharp
{
    public interface IBuffGameScript
    {
        IBuffScriptMetaData BuffMetaData { get; }
        IStatsModifier StatsModifier { get; }

        void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
        }
        void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
        }
        void OnUpdate(float diff)
        {
        }
    }
}
