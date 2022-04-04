using GameServerCore.Content;
using GameServerCore.Domain.GameObjects;
using LeaguePackets.Game.Common;
using LeaguePackets.Game.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PacketDefinitions420
{
    public static class PacketExtensions
    {
        /// <summary>
        /// Converts the given list of Vector2s into a list of CompressedWaypoints compatible with LeaguePackets, which are Vector2s with their origin at the center of the map.
        /// </summary>
        /// <param name="wp">List of Vector2s to convert.</param>
        /// <param name="grid">NavigationGrid to use for conversion.</param>
        /// <returns>List of CompressedWaypoints (Vector2s with origin at the center of the map).</returns>
        public static List<CompressedWaypoint> Vector2ToWaypoint(List<Vector2> wp, INavigationGrid grid)
        {
            return wp.ConvertAll(v => Vector2ToWaypoint(TranslateToCenteredCoordinates(v, grid)));
        }

        /// <summary>
        /// Converts the given CompressedWaypoint into a Vector2, however it does not unconvert it, meaning it will still have its origin at the center of the map.
        /// </summary>
        /// <param name="cw">CompressedWaypoint to convert.</param>
        /// <returns>Vector2 with equivalent coordinates.</returns>
        public static Vector2 WaypointToVector2(CompressedWaypoint cw)
        {
            return new Vector2(cw.X, cw.Y);
        }

        /// <summary>
        /// Converts the given Vector2 into a CompressedWaypoint, however the origin is not converted. Vector2 must have its origin at the center of the map before conversion.
        /// </summary>
        /// <param name="cw">Vector2 to convert.</param>
        /// <returns>CompressedWaypoint with equivalent coordinates.</returns>
        public static CompressedWaypoint Vector2ToWaypoint(Vector2 cw)
        {
            return new CompressedWaypoint((short)cw.X, (short)cw.Y);
        }

        /// <summary>
        /// Converts the given Vector2 back into a Vector2 with an origin at the bottom left corner of the map.
        /// </summary>
        /// <param name="vector">Vector2 to convert.</param>
        /// <param name="grid">NavigationGrid used for grabbing center of the map.</param>
        /// <returns>Vector2 with origin at the center of the map.</returns>
        public static Vector2 TranslateFromCenteredCoordinates(Vector2 vector, INavigationGrid grid)
        {
            // For unk reason coordinates are translated to 0,0 as a map center, so we gotta get back the original
            // mapSize contains the real center point coordinates, meaning width/2, height/2
            return new Vector2(2 * vector.X + grid.MiddleOfMap.X, 2 * vector.Y + grid.MiddleOfMap.Y);
        }

        /// <summary>
        /// Converts the given Vector2 into a Vector2 with an origin at the center of the map.
        /// </summary>
        /// <param name="vector">Vector2 to convert.</param>
        /// <param name="grid">NavigationGrid used for grabbing center of the map.</param>
        /// <returns>Vector2 with origin at the center of the map.</returns>
        public static Vector2 TranslateToCenteredCoordinates(Vector2 vector, INavigationGrid grid)
        {
            // For unk reason coordinates are translated to 0,0 as a map center, so we gotta get back the original
            // mapSize contains the real center point coordinates, meaning width/2, height/2
            return new Vector2((vector.X - grid.MiddleOfMap.X) / 2, (vector.Y - grid.MiddleOfMap.Y) / 2);
        }
        public static IEventEmptyHistory GetAnnouncementID(GameServerCore.Enums.EventID Event, int mapId = 0)
        {
            var worldEvent = (EventID)(byte)Event;
            switch (worldEvent)
            {
                case EventID.OnStartGameMessage1:
                    return new OnStartGameMessage1 { MapNumber = mapId };
                case EventID.OnStartGameMessage2:
                    return new OnStartGameMessage2 { MapNumber = mapId };
                case EventID.OnStartGameMessage3:
                    return new OnStartGameMessage3 { MapNumber = mapId };
                case EventID.OnStartGameMessage4:
                    return new OnStartGameMessage4 { MapNumber = mapId };
                case EventID.OnStartGameMessage5:
                    return new OnStartGameMessage5 { MapNumber = mapId };
                case EventID.OnMinionsSpawn:
                    return new OnMinionsSpawn();
                case EventID.OnNexusCrystalStart:
                    return new OnNexusCrystalStart();
                case EventID.OnMinionAscended:
                    return new OnMinionAscended();
                case EventID.OnChampionAscended:
                    return new OnChampionAscended();
                case EventID.OnClearAscended:
                    return new OnClearAscended();
            }
            return null;
        }

        /// <summary>
        /// Creates the MovementData for the given MovementDataType.
        /// </summary>
        /// <param name="o">GameObject to create MovementData for.</param>
        /// <param name="grid">NavigationGrid used for grabbing center of the map.</param>
        /// <param name="type">Type of MovementData to create.</param>
        /// <returns>Default MovementDataStop, otherwise: If GameObject, None or Stop. If AttackableUnit, all of the above.</returns>
        public static MovementData CreateMovementData(IGameObject o, INavigationGrid grid, MovementDataType type, SpeedParams speeds = null, bool useTeleportID = false)
        {
            MovementData md = new MovementDataStop
            {
                SyncID = (int)o.SyncId,
                Position = o.Position,
                Forward = new Vector2(o.Direction.X, o.Direction.Z)
            };

            switch (type)
            {
                case MovementDataType.None:
                {
                    md = new MovementDataNone
                    {
                        SyncID = (int)o.SyncId
                    };

                    return md;
                }
                case MovementDataType.Stop:
                {
                    return md;
                }
            }

            if (o is IAttackableUnit unit)
            {
                // Prevent 0 waypoints packet error.
                if (unit.Waypoints.Count < 1)
                {
                    return md;
                }

                var currentWaypoints = new List<Vector2>(unit.Waypoints);
                currentWaypoints[0] = unit.Position;

                int count = 2 + ((currentWaypoints.Count - 1) - unit.CurrentWaypoint.Key);
                if (count >= 2)
                {
                    currentWaypoints.RemoveRange(1, currentWaypoints.Count - count);
                }
                
                var waypoints = currentWaypoints.ConvertAll(v => Vector2ToWaypoint(TranslateToCenteredCoordinates(v, grid)));

                switch (type)
                {
                    case MovementDataType.WithSpeed:
                    {
                        if (speeds != null)
                        {
                            md = new MovementDataWithSpeed
                            {
                                SyncID = unit.SyncId,
                                TeleportNetID = unit.NetId,
                                HasTeleportID = useTeleportID,
                                TeleportID = useTeleportID ? unit.TeleportID : (byte)0,
                                Waypoints = waypoints,
                                SpeedParams = speeds
                            };
                        }

                        break;
                    }
                    case MovementDataType.Normal:
                    {
                        md = new MovementDataNormal
                        {
                            SyncID = unit.SyncId,
                            TeleportNetID = unit.NetId,
                            HasTeleportID = useTeleportID,
                            TeleportID = useTeleportID ? unit.TeleportID : (byte)0,
                            Waypoints = waypoints
                        };

                        break;
                    }
                }
            }

            return md;
        }
    }
}
