using System.Numerics;
using GameServerCore.Domain.GameObjects;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;

namespace Spells
{
    public class SummonerFlash : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            // TODO
        };

        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
            var current = new Vector2(owner.Position.X, owner.Position.Y);
            var to = start - current;
            Vector2 trueCoords;

            if (to.Length() > 425)
            {
                to = Vector2.Normalize(to);
                var range = to * 425;
                trueCoords = current + range;
            }
            else
            {
                trueCoords = start;
            }

            owner.FaceDirection(new Vector3(to.X, 0.0f, to.Y));

            AddParticle(owner, "global_ss_flash.troy", owner.Position);
            TeleportTo(owner, trueCoords.X, trueCoords.Y);
            AddParticleTarget(owner, "global_ss_flash_02.troy", owner);
        }

        public void OnSpellCast(ISpell spell)
        {
        }

        public void OnSpellPostCast(ISpell spell)
        {
        }

        public void OnUpdate(float diff)
        {
        }
    }
}

