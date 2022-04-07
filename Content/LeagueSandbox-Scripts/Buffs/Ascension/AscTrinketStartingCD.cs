using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace Buffs
{
    internal class AscTrinketStartingCD : IBuffGameScript
    {
        public IBuffScriptMetaData BuffMetaData { get; set; } = new BuffScriptMetaData
        {
            BuffType = BuffType.INTERNAL,
            BuffAddType = BuffAddType.REPLACE_EXISTING
        };
        public IStatsModifier StatsModifier { get; private set; }


        public void OnActivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
        } 

        public void OnDeactivate(IAttackableUnit unit, IBuff buff, ISpell ownerSpell)
        {
            if (unit is IObjAiBase obj && obj.Inventory != null)
            {
                obj.Spells[6 + (byte)SpellSlotType.InventorySlots].SetCooldown(45.0f, true);
            }
        }

        public void OnUpdate(float diff)
        {
        }
    }
}