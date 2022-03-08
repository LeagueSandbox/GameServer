using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.API;
using GameServerCore.Domain;

namespace CharScripts
{
    internal class CharScriptSRU_Baron : ICharScript
    {
        public void OnActivate(IObjAiBase owner, ISpell spell = null)
        {
            //TODDO: Make so it can't face other direction
            SetStatus(owner, StatusFlags.CanMove, false);

            //TODO: Implement these buff Scripts
            AddBuff("ResistantSkin", 25000.0f, 1, null, owner, owner, false);
            AddBuff("BaronCorruption", 25000.0f, 1, null, owner, owner, false);
            
            if(owner is IMonster)
            {
                ApiEventManager.OnDeath.AddListener(this, owner, OnDeath, true);
            }
        }

        public void OnDeath(IDeathData deathData)
        {
            foreach (var player in GetAllPlayersFromTeam(deathData.Killer.Team))
            {
                if (!player.IsDead)
                {
                    AddBuff("ExaltedWithBaronNashor", 240.0f, 1, null, player, deathData.Unit as IMonster);
                }
                player.AddGold(player, 300);
            }

            foreach(var unit in GetUnitsInRange(deathData.Unit.Position, 1000.0f, true))
            {
                if(unit is IMonster mons && mons.Name == "SRU_BaronSpawn12.1.2")
                {
                    mons.TakeDamage(mons, 100000.0f, DamageType.DAMAGE_TYPE_TRUE, DamageSource.DAMAGE_SOURCE_INTERNALRAW, false);
                }
            }
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell = null)
        {
        }

        public void OnUpdate(float diff)
        {
        }
    }
}
