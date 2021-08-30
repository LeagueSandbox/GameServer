using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using System.Numerics;

namespace GameServerCore.Scripting.CSharp
{
    public interface ICharScript
    {
        void OnActivate(IObjAiBase owner, ISpell spell);

        void OnDeactivate(IObjAiBase owner, ISpell spell);

        void OnUpdate(float diff);
    }
}
