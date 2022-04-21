using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using System.Numerics;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI
{
    public class Pet : Minion, IPet
    {
        private float _returnRadius;

        public IBuff CloneBuff { get; }
        public ISpell SourceSpell { get; }
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
            int skinId = 0,
            bool cloneInventory = true,
            bool showMinimapIfClone = true,
            bool disallowPlayerControl = false,
            bool doFade = false,
            bool isClone = true,
            string aiScript = "Pet"
            ) : base(game, owner, position, model, name, team: owner.Team, skinId: skinId,aiScript: aiScript)
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

            game.ObjectManager.AddObject(this);
        }

        public override void OnAdded()
        {
            base.OnAdded();
            AddBuff(CloneBuff);
            Owner.SetPet(this);
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
