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
        private IObjAiBase _owner;
        private ISpell _spell;
        private float Damage = 0f;
        public ISpellScriptMetadata ScriptMetadata { get; private set; } = new SpellScriptMetadata()
        {
            // TODO
        };

        private void TargetExecute(IDamageData data)
        {
            data.Target.TakeDamage(this._owner, Damage, DamageType.DAMAGE_TYPE_MAGICAL, DamageSource.DAMAGE_SOURCE_RAW, false);
        }


        public void OnUpdate(float diff)
        {
            Damage = 15 + (this._owner.Stats.AbilityPower.Total * 0.15f);

            // Getting item slots is a bit of a mess right now. Maybe add a function in API to get?
            SetSpellToolTipVar(_owner, 0, Damage, SpellbookType.SPELLBOOK_CHAMPION, _owner.Inventory.GetItemSlot(_owner.Inventory.GetItem("Malady")), SpellSlotType.InventorySlots);
        }

        public void OnActivate(IObjAiBase owner, ISpell spell)
        {
            _owner = owner;
            _spell = spell;
            ApiEventManager.OnHitUnit.AddListener(this, owner, TargetExecute, false);
        }

        public void OnDeactivate(IObjAiBase owner, ISpell spell)
        {
            ApiEventManager.OnHitUnit.RemoveListener(this, owner);
        }

        public void OnSpellPreCast(IObjAiBase owner, ISpell spell, IAttackableUnit target, Vector2 start, Vector2 end)
        {

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
    }
}
