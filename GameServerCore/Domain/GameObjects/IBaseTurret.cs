using GameServerCore.Enums;

namespace GameServerCore.Domain.GameObjects
{
    /// <summary>
    /// Base class for Turret GameObjects.
    /// In League, turrets are separated into visual and AI objects, so this GameObject represents the AI portion,
    /// while the visual object is handled automatically by clients via packets.
    /// </summary>
    public interface IBaseTurret : IObjAiBase
    {
        /// <summary>
        /// Current lane this turret belongs to.
        /// </summary>
        LaneID Lane { get; }
        /// <summary>
        /// Supposed to be the NetID of the building component of this turret (the collision object).
        /// </summary>
        uint ParentNetId { get; }
        /// <summary>
        /// Internal name of this turret.
        /// </summary>
        string Name { get; }
        /// <summary>
        /// Sets this turret's LaneID to the specified LaneID.
        /// Only sets if its current LaneID is NONE.
        /// Used for ObjectManager.
        /// </summary>
        /// <param name="newId"></param>
        void SetLaneID(LaneID newId);
    }
}
