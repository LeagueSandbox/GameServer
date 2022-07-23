using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;

namespace GameServerCore.Scripting.CSharp
{
    public interface ICharScript
    {
        void OnActivate(ObjAIBase owner, Spell spell = null)
        {
        }

        void OnDeactivate(ObjAIBase owner, Spell spell = null)
        {
        }

        void OnUpdate(float diff)
        {
        }
    }
}
