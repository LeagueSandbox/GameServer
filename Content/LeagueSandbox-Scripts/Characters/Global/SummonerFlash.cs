using System.Numerics;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Scripting.CSharp;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;

namespace Spells
{
    public class SummonerFlash : ISpellScript
    {
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            CastingBreaksStealth = false,
            TriggersSpellCasts = false,
            NotSingleTargetSpell = true
            // TODO
        };

        public void OnSpellPreCast(ObjAIBase owner, Spell spell, AttackableUnit target, Vector2 start, Vector2 end)
        {
            var current = owner.Position;
            var dist = Vector2.Distance(current, start);

            FaceDirection(start, owner, true);

            if (dist > spell.SpellData.CastRangeDisplayOverride)
            {
                start = GetPointFromUnit(owner, spell.SpellData.CastRangeDisplayOverride);
            }

            StopChanneling(owner, ChannelingStopCondition.Cancel, ChannelingStopSource.Move);

            AddParticle(owner, null, "global_ss_flash", owner.Position);
            AddParticleTarget(owner, owner, "global_ss_flash_02", owner);

            TeleportTo(owner, start.X, start.Y);
        }
    }
}

