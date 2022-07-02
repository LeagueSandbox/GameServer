using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;

namespace GameServerCore.Scripting.CSharp
{
    public interface ICharScript
    {
        void OnActivate(IObjAIBase owner, ISpell spell = null)
        {
        }

        void OnDeactivate(IObjAIBase owner, ISpell spell = null)
        {
        }

        void OnUpdate(float diff)
        {
        }
    }
}
