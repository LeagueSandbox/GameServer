using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using System.Numerics;

namespace ItemSpells
{
    public class Malady : ISpellScript
    {
        private IObjAIBase _owner;
        private ISpell _spell;
        private float _damage = 0f;
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            // TODO
        };

        private void TargetExecute(IDamageData data)
        {
            data.Target.TakeDamage(_owner, _damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_RAW, false);
        }

        private void OnStatsUpdate(IAttackableUnit _unit, float _delta)
        {
            float damage = 15 + (_owner.Stats.AbilityPower.Total * 0.15f);
            if(_damage != damage)
            {
                _damage = damage;
                // Getting item slots is a bit of a mess right now. Maybe add a function in API to get?
                SetSpellToolTipVar(_owner, 0, _damage, SpellbookType.SPELLBOOK_CHAMPION, _owner.Inventory.GetItemSlot(_owner.Inventory.GetItem("Malady")), SpellSlotType.InventorySlots);
            }
        }

        public void OnActivate(IObjAIBase owner, ISpell spell)
        {
            _owner = owner;
            _spell = spell;
            ApiEventManager.OnHitUnit.AddListener(this, owner, TargetExecute, false);
            ApiEventManager.OnUpdateStats.AddListener(this, owner, OnStatsUpdate, false);
        }

        public void OnDeactivate(IObjAIBase owner, ISpell spell)
        {
            ApiEventManager.OnHitUnit.RemoveListener(this, owner);
        }
    }
}
