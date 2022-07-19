using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;

namespace Spells
{
    public class BasePetController : ISpellScript
    {
        private Pet Pet;
        public SpellScriptMetadata ScriptMetadata => new SpellScriptMetadata()
        {
            NotSingleTargetSpell = true,
            DoesntBreakShields = true,
            TriggersSpellCasts = false,
            IsDamagingSpell = true,
            SpellDamageRatio = 0.5f,
            IsPetDurationBuff = true
        };

        public void OnActivate(ObjAIBase owner, Spell spell)
        {
            Pet = owner.GetPet();
        }

        public void OnSpellPreCast(ObjAIBase owner, Spell spell, AttackableUnit target, Vector2 start, Vector2 end)
        {
            if (Pet != null)
            {
                // likely AddBuff("PetCommandParticle") here (refer to preload for particles)
                //This particle get's displayed globably
                AddParticle(owner, null, "cursor_moveto", start);

                //TODO: Instead of baking AI here, make a general Pet AI script and set it as the default AI for Pet class.
                var unitsInRange = GetUnitsInRange(end, 100.0f, true);
                unitsInRange.RemoveAll(x => x.Team == spell.CastInfo.Owner.Team);
                if (unitsInRange.Count > 0)
                {
                    Pet.UpdateMoveOrder(OrderType.PetHardAttack);
                    Pet.SetTargetUnit(unitsInRange[0]);
                    for (int i = 0; i < unitsInRange.Count; i++)
                    {
                        spell.CastInfo.SetTarget(unitsInRange[i], i);
                    }
                }
                else
                {
                    Pet.SetTargetUnit(null, true);
                    Pet.UpdateMoveOrder(OrderType.PetHardMove);
                    Pet.SetWaypoints(GetPath(Pet.Position, end));
                }
            }
        }
    }
}
