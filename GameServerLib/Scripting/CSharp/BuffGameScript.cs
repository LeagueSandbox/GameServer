using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.Spells;

namespace LeagueSandbox.GameServer.Scripting.CSharp
{
    public interface IBuffGameScript
    {
        void OnUpdate(double diff);

        void OnActivate(IObjAiBase unit, ISpell ownerSpell);

        void OnDeactivate(IObjAiBase unit);
    }
}
