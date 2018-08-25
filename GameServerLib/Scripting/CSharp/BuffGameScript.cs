using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.Spells;

namespace LeagueSandbox.GameServer.Scripting.CSharp
{
    public interface IBuffGameScript
    {
        void OnUpdate(double diff);

        void OnActivate(ObjAiBase unit, Spell ownerSpell);

        void OnDeactivate(ObjAiBase unit);
    }
}
