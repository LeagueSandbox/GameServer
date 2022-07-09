using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using System;
using System.Numerics;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI
{
    public class Pet : Minion, IPet
    {
        private float _returnRadius;

        /// <summary>
        /// Entity that the pet is cloning (Ex. Who Mordekaiser's ghost is)
        /// </summary>
        public IObjAIBase ClonedUnit { get; private set; }
        /// <summary>
        /// Buff Assigned to this Pet at Spawn
        /// </summary>
        public IBuff CloneBuff { get; }
        /// <summary>
        /// Spell that created this Pet
        /// </summary>
        public ISpell SourceSpell { get; }
        /// <summary>
        /// Duration of CloneBuff
        /// </summary>
        public float LifeTime { get; }
        public bool CloneInventory { get; }
        public bool DoFade { get; }
        public bool ShowMinimapIconIfClone { get; }
        public bool DisallowPlayerControl { get; }
        public bool IsClone { get; }

        public Pet(
            Game game,
            IChampion owner,
            ISpell spell,
            Vector2 position,
            string name,
            string model,
            string buffName,
            float lifeTime,
            IStats stats = null,
            bool cloneInventory = true,
            bool showMinimapIfClone = true,
            bool disallowPlayerControl = false,
            bool doFade = false,
            bool isClone = true,
            string AIScript = "Pet"
            ) : base(game, owner, position, model, name, 0, owner.Team, owner.SkinID, stats: stats, AIScript: AIScript)
        {
            _returnRadius = _game.Map.MapScript.MapScriptMetadata.AIVars.DefaultPetReturnRadius;

            SourceSpell = spell;
            LifeTime = lifeTime;
            CloneBuff = new Buff(game, buffName, lifeTime, 1, spell, this, owner);
            CloneInventory = cloneInventory;
            ShowMinimapIconIfClone = showMinimapIfClone;
            DisallowPlayerControl = disallowPlayerControl;
            DoFade = doFade;
            IsClone = isClone;

            Owner.SetPet(this);
            game.ObjectManager.AddObject(this);
        }

        public Pet(
            Game game,
            IChampion owner,
            ISpell spell,
            IObjAIBase cloned,
            Vector2 position,
            string buffName,
            float lifeTime,
            IStats stats = null,
            bool cloneInventory = true,
            bool showMinimapIfClone = true,
            bool disallowPlayerControl = false,
            bool doFade = false,
            string AIScript = "Pet"
            ) : base(game, owner, cloned.Position, cloned.Model, cloned.Name, 0, owner.Team, cloned.SkinID, stats: stats, AIScript: AIScript)
        {

            if (position == Vector2.Zero)
            {
                Position = cloned.Position;
            }
            else
            {
                Position = position;
            }

            SourceSpell = spell;
            LifeTime = lifeTime;
            ClonedUnit = cloned;
            CloneBuff = new Buff(game, buffName, lifeTime, 1, spell, this, owner);
            CloneInventory = cloneInventory;
            ShowMinimapIconIfClone = showMinimapIfClone;
            DisallowPlayerControl = disallowPlayerControl;
            DoFade = doFade;
            IsClone = true;

            Owner.SetPet(this);
            game.ObjectManager.AddObject(this);
        }

        public override void OnAdded()
        {
            base.OnAdded();
            AddBuff(CloneBuff);

            if(CloneInventory && ClonedUnit != null)
            {
                foreach(var item in ClonedUnit.Inventory.GetAllItems())
                {
                    Inventory.AddItem(item.ItemData, this);
                }
            }
        }

        public float GetReturnRadius()
        {
            return _returnRadius;
        }

        public void SetReturnRadius(float radius)
        {
            _returnRadius = radius;
        }
    }
}
