using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore;
using System.Linq;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;

namespace Spells
{
    public class RaiseMorale : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
            // TODO
        };

        public void OnSpellPostCast(Spell spell)
        {		
            var hasbuff = spell.CastInfo.Owner.HasBuff("GangplankE");
            
            if (hasbuff == false)
            {
                AddBuff("GangplankE", 7.0f, 1, spell, spell.CastInfo.Owner, spell.CastInfo.Owner);
            }

            var units = GetUnitsInRange(spell.CastInfo.Owner.Position, 1000, true).Where(x => x.Team != CustomConvert.GetEnemyTeam(spell.CastInfo.Owner.Team));

            foreach (var allyTarget in units)
            {
                if (allyTarget is AttackableUnit && spell.CastInfo.Owner != allyTarget && hasbuff == false)
                {
                    AddBuff("GangplankE", 7.0f, 1, spell, allyTarget, spell.CastInfo.Owner);
                }
            }
        }
    }
}
