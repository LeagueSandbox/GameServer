using GameServerCore.Enums;
using System.Numerics;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;

namespace GameServerCore.Scripting.CSharp
{
    public interface ISpellScript
    {
        SpellScriptMetadata ScriptMetadata { get; }

        void OnActivate(ObjAIBase owner, Spell spell)
        {
        }

        void OnDeactivate(ObjAIBase owner, Spell spell)
        {
        }

        void OnSpellPreCast(ObjAIBase owner, Spell spell, AttackableUnit target, Vector2 start, Vector2 end)
        {
        }

        void OnSpellCast(Spell spell)
        {
        }

        void OnSpellPostCast(Spell spell)
        {
        }

        void OnSpellChannel(Spell spell)
        {
        }

        void OnSpellChannelCancel(Spell spell, ChannelingStopSource reason)
        {
        }

        void OnSpellPostChannel(Spell spell)
        {
        }

        void OnUpdate(float diff)
        {            
        }
    }
}
