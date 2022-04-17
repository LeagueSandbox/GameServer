using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI
{
    public class Pet : Minion, IPet
    {
        public IBuff CloneBuff { get; }
        public float LifeTime { get; }
        public bool CloneInventory { get; }
        public bool DoFade { get; }
        public bool ShowMinimapIfClone { get; }
        public Pet(
            Game game,
            IChampion owner,
            ISpell spell,
            Vector2 position,
            string name,
            string model,
            string buffName,
            float lifeTime,
            bool cloneInventory = true,
            bool showMinimapIfClone = true,
            bool doFade = false
            ) : base(game, owner, position, model, name, team: owner.Team)
        {
            CloneBuff = new Buff(game, buffName, lifeTime, 1, spell, owner, owner);
            AddBuff(CloneBuff);
            ShowMinimapIfClone = showMinimapIfClone;
            DoFade = doFade;
            LifeTime = lifeTime;
            CloneInventory = cloneInventory;
            
            //This actually seems to be handled on-script, since Tibbers has CloneInventory set as true, but doesn't actually clone the inventory
            /*if (CloneInventory)
            {
                foreach(var item in owner.Inventory.GetAllItems())
                {
                    Inventory.AddItemToSlot(item.ItemData, this, owner.Inventory.GetItemSlot(item));
                }
            }*/
            game.ObjectManager.AddObject(this);
        }

    }
}
