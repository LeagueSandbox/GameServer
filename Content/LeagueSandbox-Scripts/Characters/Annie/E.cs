using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;

namespace Spells
{
    public class MoltenShield : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata => new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
        };

        public void OnSpellPostCast(Spell spell)
        {
            AddBuff("MoltenShield", 5.0f, 1, spell, spell.CastInfo.Owner, spell.CastInfo.Owner);
            if (spell.CastInfo.Owner is Champion ch)
            {
                Pet tibbers = ch.GetPet();
                if (tibbers != null && !tibbers.IsDead)
                {
                    AddBuff("MoltenShield", 5.0f, 1, spell, tibbers, spell.CastInfo.Owner);
                }
            }
        }
    }
}