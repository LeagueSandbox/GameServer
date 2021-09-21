using System.Numerics;

namespace GameServerCore.Domain.GameObjects.Spell.Sector
{
    public interface ISpellSectorPolygon : ISpellSector
    {
        Vector2[] GetPolygonVertices();
    }
}
