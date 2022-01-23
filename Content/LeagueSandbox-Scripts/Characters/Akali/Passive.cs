using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using System.Numerics;
using GameServerCore.Scripting.CSharp;

namespace CharScripts
{
    public class CharScriptAkali : ICharScript
    {
        public void OnActivate(IObjAiBase owner, ISpell spell = null)
        {
            var bonusAd = owner.Stats.AttackDamage.Total - owner.Stats.AttackDamage.BaseValue;
            owner.Stats.SpellVamp.PercentBonus = 6 + bonusAd % 6;
        }
        public void OnDeactivate(IObjAiBase owner, ISpell spell = null)
        {
        }
        public void OnUpdate(float diff)
        {
        }
    }
}

