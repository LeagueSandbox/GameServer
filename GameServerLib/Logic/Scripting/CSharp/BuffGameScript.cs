using LeagueSandbox.GameServer.Logic.GameObjects;

namespace LeagueSandbox.GameServer.Logic.Scripting
{
    public interface IBuffGameScript
    {
        void OnUpdate(double diff);

        void OnActivate(ObjAiBase unit, Spell ownerSpell);

        void OnDeactivate(ObjAiBase unit);
    }
}
