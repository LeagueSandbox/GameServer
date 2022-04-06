using GameServerCore.Domain.GameObjects;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.Stats;
using GameServerCore.Enums;

namespace ItemPassives
{
    public class ItemID_3460 : IItemScript
    {
        public IStatsModifier StatsModifier { get; private set; } = new StatsModifier();

        IObjAiBase Owner; 
        public void OnActivate(IObjAiBase owner)
        {
            Owner = owner;
        }

        public void OnDeactivate(IObjAiBase owner)
        {
          
        }

        //Have to delay the cooldown notification due to: https://github.com/LeagueSandbox/GameServer/issues/1387
        int tickDelay = 0;
        public void OnUpdate(float diff)
        {
            if(tickDelay <= 1)
            {
                Owner.Spells[6 + (byte)SpellSlotType.InventorySlots].SetCooldown(45, true);
                tickDelay++;
            }
        }
    }
}

