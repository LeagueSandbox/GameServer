using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using            GameServerLib.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.SpellNS;

namespace ItemSpells
{
    public class Malady : ISpellScript
    {
        private ObjAIBase _owner;
        private Spell _spell;
        private float _damage = 0f;
        public SpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            // TODO
        };

        private void TargetExecute(DamageData data)
        {
            data.Target.TakeDamage(_owner, _damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_RAW, false);
        }

        private void OnStatsUpdate(AttackableUnit _unit, float _delta)
        {
            float damage = 15 + (_owner.Stats.AbilityPower.Total * 0.15f);
            if(_damage != damage)
            {
                _damage = damage;
                // Getting item slots is a bit of a mess right now. Maybe add a function in API to get?
                SetSpellToolTipVar(_owner, 0, _damage, SpellbookType.SPELLBOOK_CHAMPION, _owner.Inventory.GetItemSlot(_owner.Inventory.GetItem("Malady")), SpellSlotType.InventorySlots);
            }
        }

        public void OnActivate(ObjAIBase owner, Spell spell)
        {
            _owner = owner;
            _spell = spell;
            ApiEventManager.OnHitUnit.AddListener(this, owner, TargetExecute, false);
            ApiEventManager.OnUpdateStats.AddListener(this, owner, OnStatsUpdate, false);
        }

        public void OnDeactivate(ObjAIBase owner, Spell spell)
        {
            ApiEventManager.OnHitUnit.RemoveListener(this, owner);
        }
    }
}
