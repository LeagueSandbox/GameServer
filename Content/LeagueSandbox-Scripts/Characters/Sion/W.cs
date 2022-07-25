using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using            GameServerLib.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.SpellNS;
using LeagueSandbox.GameServer.GameObjects.StatsNS;

namespace Spells
{
    public class SionW : ISpellScript
    {
        public StatsModifier StatsModifier { get; private set; } = new StatsModifier();
        Spell thisSpell;

        public SpellScriptMetadata ScriptMetadata => new SpellScriptMetadata()
        {
            TriggersSpellCasts = true
            // TODO
        };

        public void OnActivate(ObjAIBase owner, Spell spell)
        {
            ApiEventManager.OnKillUnit.AddListener(this, owner, OnKillMinion, false);
            ApiEventManager.OnKill.AddListener(this, owner, OnKillChampion, false);
            thisSpell = spell;
        }
        public void OnKillMinion(DeathData deathData)
        {
            if (thisSpell.CastInfo.SpellLevel >= 1)
            {
                var owner = deathData.Killer;
                float extraHealth = 2f;

                StatsModifier.HealthPoints.FlatBonus = extraHealth;
                deathData.Killer.AddStatModifier(StatsModifier);
                owner.Stats.CurrentHealth += extraHealth;
            }
        }
        public void OnKillChampion(DeathData deathData)
        {
            if (thisSpell.CastInfo.SpellLevel >= 1)
            {
                var owner = deathData.Killer;
                float extraHealth = 10f;

                StatsModifier.HealthPoints.FlatBonus = extraHealth;
                owner.AddStatModifier(StatsModifier);
                owner.Stats.CurrentHealth += extraHealth;
            }
        }
    }
}

