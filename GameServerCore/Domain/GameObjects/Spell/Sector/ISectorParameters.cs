using GameServerCore.Enums;
using System.Numerics;

namespace GameServerCore.Domain.GameObjects.Spell.Sector
{
    /// <summary>
    /// Parameters which determine how a sector behaves.
    /// </summary>
    public interface ISectorParameters
    {
        /// <summary>
        /// Optional object the sector should be bound to. The sector will be attached to this object and will use its facing direction.
        /// </summary>
        IGameObject BindObject { get; set; }
        /// <summary>
        /// Distance from the bottom of the sector to the top.
        /// If this is larger than Width, it will be used as the area around the sector to check for collisions.
        /// Scales the distance (in y) between PolygonVertices.
        /// </summary>
        float Length { get; set; }
        /// <summary>
        /// Distance from the left side of the sector to the right side.
        /// If this is larger than Length, it will be used as the area around the sector to check for collisions.
        /// Scales the distance (in x) between PolygonVertices.
        /// </summary>
        float Width { get; set; }
        /// <summary>
        /// If the Type is Cone, this will filter collisions that are in front of the sector and are within this angle from the sector's center.
        /// Should be a value from 0->360
        /// </summary>
        float ConeAngle { get; set; }
        /// <summary>
        /// If the Type is Polygon, this will represent the vertices of the sector.
        /// Vertices are relative to the origin (SpellCastLaunchPosition or target position & direction).
        /// If the distance between points exceeds HalfLength/Width, that distance will be used instead as the collision radius check for the sector.
        /// Points should be ordered such that each point connects to the next (with the last point connecting to the first point).
        /// Due to HalfLength and Width scaling the distance between vertices, it is recommended that points be arranged with x and y values between 0 and 1.
        /// </summary>
        Vector2[] PolygonVertices { get; set; }
        /// <summary>
        /// Maximum amount of time this spell sector should last (in seconds) before being automatically removed.
        /// Setting to -1 will cause the spell sector to last until manually removed.
        /// </summary>
        float Lifetime { get; set; }
        /// <summary>
        /// Whether or not the sector should only tick once before being removed (Lifetime must be greater than a single tick).
        /// </summary>
        bool SingleTick { get; set; }
        /// <summary>
        /// How many times a second the spell sector should check for hitbox collisions.
        /// </summary>
        int Tickrate { get; set; }
        /// <summary>
        /// Whether or not the spell sector should be able to hit something multiple times.
        /// Will only hit again if the unit hit re-enters the hitbox (constant per-collision hitbox).
        /// Is overridden by CanHitSameTargetConsecutively.
        /// </summary>
        bool CanHitSameTarget { get; set; }
        /// <summary>
        /// Whether or not the spell sector should be able to hit something multiple times in a row,
        /// regardless of if it has left and re-entered the hitbox (costant hitbox).
        /// Overrides CanHitSameTarget.
        /// </summary>
        bool CanHitSameTargetConsecutively { get; set; }

        /// <summary>
        /// Maximum number of times the spell sector can hit something before being removed. A value of 0 or less means this variable will be unused.
        /// </summary>
        int MaximumHits { get; set; }

        /// <summary>
        /// Optional spell flags which determine what units this spell sector affects.
        /// If 0, the sector will use the SpellOrigin's spell flags.
        /// </summary>
        SpellDataFlags OverrideFlags { get; set; }

        /// <summary>
        /// What kind of shape the sector has.
        /// </summary>
        SectorType Type { get; set; }
    }
}