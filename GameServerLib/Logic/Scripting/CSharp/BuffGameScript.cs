using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Logic.GameObjects.Spells;

namespace LeagueSandbox.GameServer.Logic.Scripting.CSharp
{
    public interface IBuffGameScript
    {
        void OnUpdate(double diff);

        void OnActivate(ObjAiBase unit, Spell ownerSpell);

        void OnDeactivate(ObjAiBase unit);
    }
}
