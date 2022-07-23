using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;

namespace Spells
{
    public class YasuoDashWrapper : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            // TODO
        };

        public static AttackableUnit _target = null;

        public void OnSpellPreCast(ObjAIBase owner, Spell spell, AttackableUnit target, Vector2 start, Vector2 end)
        {
            _target = target;
            if (!target.HasBuff("YasuoEBlock"))
            {
                AddBuff("YasuoE", 0.395f - spell.CastInfo.SpellLevel * 0.012f, 1, spell, owner, owner);
                AddBuff("YasuoEBlock", 11f - spell.CastInfo.SpellLevel * 1f, 1, spell, target, owner);
            }
        }

        public void OnSpellCast(Spell spell)
        {
            //here's empty, maybe will add some functions?
        }

        public void OnUpdate(float diff)
        {
            //here's empty because it's not working
        }
    }
}
