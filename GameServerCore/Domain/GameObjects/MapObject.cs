using GameServerCore.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

#nullable enable

namespace GameServerCore.Domain
{
    public struct MapObject : IEquatable<MapObject>
    {
        static readonly MapObject _empty = new MapObject { Name = "", CentralPoint = Vector3.Zero };

        public string Name { get; private set; }
        public Vector3 CentralPoint { get; private set; }
        public int ParentMapId { get; private set; }

        public MapObject(string name, Vector3 point, int id)
        {
            Name = name;
            CentralPoint = point;
            ParentMapId = id;
        }

        public static MapObject Empty { [MethodImpl(MethodImplOptions.AggressiveInlining)] get { return _empty; } }

        //
        // Summary:
        //     Returns the hash code for this instance.
        //
        // Returns:
        //     The hash code.
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        //
        // Summary:
        //     Returns the string representation of the current instance using the specified
        //     format string to format individual elements.
        //
        // Parameters:
        //   format:
        //     A standard or custom numeric format string that defines the format of individual
        //     elements.
        //
        // Returns:
        //     The string representation of the current instance.
        public override string? ToString()
        {
            return base.ToString();
        }

        //
        // Summary:
        //     Returns a value that indicates whether this instance and a specified object are
        //     equal.
        //
        // Parameters:
        //   obj:
        //     The object to compare with the current instance.
        //
        // Returns:
        //     true if the current instance and obj are equal; otherwise, false. If obj is null,
        //     the method returns false.
        public readonly override bool Equals(object? obj)
        {
            if (obj == null || !(obj is MapObject))
                return false;
            else
                return Equals((MapObject)obj);
        }

        //
        // Summary:
        //     Returns a value that indicates whether this instance and another vector are equal.
        //
        // Parameters:
        //   other:
        //     The other vector.
        //
        // Returns:
        //     true if the two vectors are equal; otherwise, false.
        public readonly bool Equals(MapObject other)
        {
            return Name == other.Name && CentralPoint == other.CentralPoint && ParentMapId == other.ParentMapId;
        }

        //
        // Summary:
        //     Returns a value that indicates whether each pair of elements in two specified
        //     vectors is equal.
        //
        // Parameters:
        //   left:
        //     The first vector to compare.
        //
        //   right:
        //     The second vector to compare.
        //
        // Returns:
        //     true if left and right are equal; otherwise, false.
        public static bool operator ==(MapObject left, MapObject right)
        {
            return left.Equals(right);
        }

        //
        // Summary:
        //     Returns a value that indicates whether two specified vectors are not equal.
        //
        // Parameters:
        //   left:
        //     The first vector to compare.
        //
        //   right:
        //     The second vector to compare.
        //
        // Returns:
        //     true if left and right are not equal; otherwise, false.
        public static bool operator !=(MapObject left, MapObject right)
        {
            return !left.Equals(right);
        }

        public GameObjectTypes GetGameObjectType()
        {
            GameObjectTypes type = 0;

            if (Name.Contains("LevelProp"))
            {
                type = GameObjectTypes.LevelProp;
            }
            else if (Name.Contains("HQ"))
            {
                type = GameObjectTypes.ObjAnimated_HQ;
            }
            else if (Name.Contains("Barracks_T"))
            {
                // Inhibitors are dampeners for the enemy Nexus.
                type = GameObjectTypes.ObjAnimated_BarracksDampener;
            }
            else if (Name.Contains("Turret"))
            {
                type = GameObjectTypes.ObjAIBase_Turret;
            }
            else if (Name.Contains("__Spawn"))
            {
                type = GameObjectTypes.ObjBuilding_SpawnPoint;
            }
            else if (Name.Contains("__NAV"))
            {
                type = GameObjectTypes.ObjBuilding_NavPoint;
            }
            else if (Name.Contains("Info_Point"))
            {
                type = GameObjectTypes.InfoPoint;
            }
            else if (Name.Contains("Shop"))
            {
                type = GameObjectTypes.ObjBuilding_Shop;
            }
            return type;
        }

        public TeamId GetTeamID()
        {
            var team = TeamId.TEAM_NEUTRAL;

            if (Name.Contains("T1") || Name.ToLower().Contains("order"))
            {
                team = TeamId.TEAM_BLUE;
            }
            else if (Name.Contains("T2") || Name.ToLower().Contains("chaos"))
            {
                team = TeamId.TEAM_PURPLE;
            }

            return team;
        }

        public TeamId GetOpposingTeamID()
        {
            var team = TeamId.TEAM_NEUTRAL;

            if (Name.Contains("T1") || Name.Contains("Order"))
            {
                team = TeamId.TEAM_PURPLE;
            }
            else if (Name.Contains("T2") || Name.Contains("Chaos"))
            {
                team = TeamId.TEAM_BLUE;
            }

            return team;
        }

        public string GetTeamName()
        {
            string teamName = "";
            if (GetTeamID() == TeamId.TEAM_BLUE)
            {
                teamName = "Order";
            }
            // Chaos and Neutral
            else
            {
                teamName = "Chaos";
            }

            return teamName;
        }

        public Lane GetLaneID()
        {
            var laneId = Lane.LANE_Unknown;

            if (Name.Contains("_L"))
            {
                laneId = Lane.LANE_L;
            }
            //Using just _C would cause files with "_Chaos" to be mistakenly assigned as MidLane
            else if (Name.Contains("_C0") || Name.Contains("_C1") || Name.Contains("_C_"))
            {
                laneId = Lane.LANE_C;
            }
            else if (Name.Contains("_R"))
            {
                laneId = Lane.LANE_R;
            }

            return laneId;
        }

        public Lane GetSpawnBarrackLaneID()
        {
            var laneId = Lane.LANE_Unknown;

            if (Name.Contains("__L"))
            {
                laneId = Lane.LANE_L;
            }
            else if (Name.Contains("__C"))
            {
                laneId = Lane.LANE_C;
            }
            else if (Name.Contains("__R"))
            {
                laneId = Lane.LANE_R;
            }

            return laneId;
        }

        public int ParseIndex()
        {
            int index = -1;

            if (GetGameObjectType() == 0)
            {
                return index;
            }

            var underscoreIndices = new List<int>();

            // While there are underscores, it loops,
            for (int i = Name.IndexOf('_'); i > -1; i = Name.IndexOf('_', i + 1))
            {
                // and ends when i = -1 (no underscore found).
                underscoreIndices.Add(i);
            }

            // If the above failed to find any underscores or the underscore is the last character in the string.
            if (underscoreIndices.Count == 0 || underscoreIndices.Last() == underscoreIndices.Count)
            {
                return index;
            }

            // Otherwise, we make a new string which starts at the last underscore (+1 character to the right),
            string startString = Name.Substring(underscoreIndices.Last() + 1);

            // and we check it for an index.
            try
            {
                index = int.Parse(startString);
            }
            catch (FormatException)
            {
                return index;
            }

            return index;
        }
    }
}