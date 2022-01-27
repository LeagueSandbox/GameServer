using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.Scripting.CSharp;
using System.Numerics;
using GameServerCore.Scripting.CSharp;
using GameServerCore.Enums;

namespace Spells
{
    public class SummonerMana : ISpellScript
    {
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            // TODO
        };

        private const float PERCENT_MAX_MANA_HEAL = 0.40f;

        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
        }

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {
            foreach (var unit in GetChampionsInRange(owner.Position, 600, true))
            {
                if (unit.Team == owner.Team)
                {
                    RestoreMana(owner);
                }
            }
        }

        private void RestoreMana(IObjAiBase target)
        {
            var maxMp = target.Stats.ManaPoints.Total;
            var newMp = target.Stats.CurrentMana + (maxMp * PERCENT_MAX_MANA_HEAL);
            if (newMp < maxMp)
                target.Stats.CurrentMana = newMp;
            else
                target.Stats.CurrentMana = maxMp;
            AddParticleTarget(target, target, "global_ss_clarity_02", target);
        }

        public void OnSpellCast(ISpell spell)
        {
        }

        public void OnSpellPostCast(ISpell spell)
        {
        }

        public void OnSpellChannel(ISpell spell)
        {
        }

        public void OnSpellChannelCancel(ISpell spell, ChannelingStopSource reason)
        {
        }

        public void OnSpellPostChannel(ISpell spell)
        {
        }

        public void OnUpdate(float diff)
        {
        }
    }
}

