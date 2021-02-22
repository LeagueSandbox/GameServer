using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore;
using System.Linq;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;

namespace Spells
{
    public class RaiseMorale : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            // TODO
        };

        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
        }

        private void SelfWasDamaged()
        {
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnStartCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {
        }

        public void OnFinishCasting(IObjAiBase owner, ISpell spell, IAttackableUnit target)
        {		
            var hasbuff = owner.HasBuff("GangplankE");
            
            if (hasbuff == false)
            {
                AddBuff("GangplankE", 7.0f, 1, spell, owner, owner);
            }

            var units = GetUnitsInRange(owner.Position, 1000, true).Where(x => x.Team != CustomConvert.GetEnemyTeam(owner.Team));

            foreach (var allyTarget in units)
            {
                if (allyTarget is IAttackableUnit && owner != allyTarget && hasbuff == false)
                {
                    AddBuff("GangplankE", 7.0f, 1, spell, allyTarget, owner);
                }
            }			
        }

        public void ApplyEffects(IObjAiBase owner, IAttackableUnit target, ISpell spell, ISpellMissile projectile)
        {
        }

        public void OnUpdate(float diff)
        {
        }
    }
}
