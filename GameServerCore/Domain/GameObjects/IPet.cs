using GameServerCore.Domain.GameObjects.Spell;

namespace GameServerCore.Domain.GameObjects
{
    public interface IPet : IMinion
    {
        IBuff CloneBuff { get; }
        ISpell SourceSpell { get; }
        float LifeTime { get; }
        bool CloneInventory { get; }
        bool DoFade { get; }
        bool ShowMinimapIconIfClone { get; }
        bool DisallowPlayerControl { get; }
        bool IsClone { get; }

        float GetReturnRadius();
        void SetReturnRadius(float radius);
    }
}
