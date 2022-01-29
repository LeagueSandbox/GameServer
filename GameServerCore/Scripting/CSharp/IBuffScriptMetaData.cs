using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Domain.GameObjects.Spell.Sector;
using GameServerCore.Enums;

namespace GameServerCore.Scripting.CSharp
{
    public interface IBuffScriptMetaData
    {
         BuffType BuffType { get; }
         BuffAddType BuffAddType { get; }
         int MaxStacks { get; }
         bool IsHidden { get; }
    }
}
