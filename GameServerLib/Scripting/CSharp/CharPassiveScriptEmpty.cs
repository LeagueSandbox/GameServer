using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Scripting.CSharp;
using System.Numerics;

namespace LeagueSandbox.GameServer.Scripting.CSharp
{
    public class CharScriptEmpty : ICharScript
    {
        public void OnActivate(IObjAIBase owner, ISpell spell = null)
        {
        }

        public void OnDeactivate(IObjAIBase owner, ISpell spell = null)
        {
        }

        public void OnUpdate(float diff)
        {
        }
    }
}
