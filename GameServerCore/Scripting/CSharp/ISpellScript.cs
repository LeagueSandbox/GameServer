using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using System.Numerics;

namespace GameServerCore.Scripting.CSharp
{
    public interface ISpellScript
    {
        ISpellScriptMetadata ScriptMetadata { get; }

        void OnActivate(IObjAIBase owner, ISpell spell)
        {
        }

        void OnDeactivate(IObjAIBase owner, ISpell spell)
        {
        }

        void OnSpellPreCast(IObjAIBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
        }

        void OnSpellCast(ISpell spell)
        {
        }

        void OnSpellPostCast(ISpell spell)
        {
        }

        void OnSpellChannel(ISpell spell)
        {
        }

        void OnSpellChannelCancel(ISpell spell, ChannelingStopSource reason)
        {
        }

        void OnSpellPostChannel(ISpell spell)
        {
        }

        void OnUpdate(float diff)
        {            
        }
    }
}
