using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;

namespace Spells
{
    public class YasuoDashWrapper : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            // TODO
        };

        public static IObjAiBase _target = null;
        public static IChampion _owner = null;
        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
            //here's nothing yet
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
            //here's empty
        }

        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            //here's empty, maybe will add some functions?
        }

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
            _target = (IObjAiBase)target;
            _owner = (IChampion)owner;

            if (!_target.HasBuff("YasuoEBlock"))
            {
                AddBuff("YasuoE", 0.395f - spell.CastInfo.SpellLevel * 0.012f, 1, spell, _owner, _owner);            
                AddBuff("YasuoEBlock", 11f - spell.CastInfo.SpellLevel * 1f, 1, spell, _target, _owner);
            }
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, ISpellMissile projectile)
        {
            //here's empty because no needed to add things here
        }
               
        public void OnUpdate(float diff)
        {
            //here's empty because it's not working
        }
    }
}
