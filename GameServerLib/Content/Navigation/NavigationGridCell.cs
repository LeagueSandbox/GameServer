using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace LeagueSandbox.GameServer.Content.Navigation
{
    /// <summary>
    /// Class representing a cell in a Navigation Grid, containing all of its features such as terrain flags, index, actors, position, and pathfinding variables.
    /// </summary>
    public class NavigationGridCell
    {
        /// <summary>
        /// Terrain flags for this cell.
        /// </summary>
        public NavigationGridCellFlags Flags { get; private set; }
        /// <summary>
        /// Index of this cell.
        /// </summary>
        public int ID { get; private set; }
        /// <summary>
        /// Whether or not this cell is open for pathing. Usually false when the cell is occupied by actors.
        /// </summary>
        public float CenterHeight { get; private set; }
        public int SessionId { get; private set; }
        public float ArrivalCost { get; private set; }
        public bool IsOpen { get; private set; }
        public float Heuristic { get; private set; }
        /// <summary>
        /// List of Actors occupying this cell, as well as how much area each Actor is occupying.
        /// </summary>
        public SortedList<GameObject, float> ActorList { get; private set; }
        /// <summary>
        /// Locator variable for this cell detailing its position on the Navigation Grid.
        /// *NOTE*: Requires translation from Navigation Grid in order to compare to League Sandbox coordinates.
        /// </summary>
        public NavigationGridLocator Locator { get; private set; }
        public float AdditionalCost { get; private set; }
        public float HintAsGoodCell { get; private set; }
        public uint AdditionalCostRefCount { get; private set; }
        public int GoodCellSessionId { get; private set; }
        public float RefHintWeight { get; private set; } = 0.5f;
        public short ArrivalDirection { get; private set; } = 9;
        public short[] RefHintNode { get; private set; } = new short[2] { -32768, -32768 };

        private NavigationGridCell() { }

        public static NavigationGridCell ReadVersion5(BinaryReader br, int id)
        {
            float centerHeight = br.ReadSingle();
            int sessionId = br.ReadInt32();
            float arrivalCost = br.ReadSingle();
            bool isOpen = br.ReadUInt32() == 1;
            float heuristic = br.ReadSingle();
            uint actorList = br.ReadUInt32();
            NavigationGridLocator locator = new NavigationGridLocator(br);
            float additionalCost = br.ReadSingle();
            float hintAsGoodCell = br.ReadSingle();
            uint additionalCostRefCount = br.ReadUInt32();
            int goodCellSessionId = br.ReadInt32();
            float refHintWeight = br.ReadSingle();
            short arrivalDirection = br.ReadInt16();
            NavigationGridCellFlags flags = (NavigationGridCellFlags)br.ReadUInt16();
            short[] refHintNode = new short[] { br.ReadInt16(), br.ReadInt16() };

            return new NavigationGridCell()
            {
                ID = id,
                Flags = flags,
                CenterHeight = centerHeight,
                SessionId = sessionId,
                ArrivalCost = arrivalCost,
                IsOpen = isOpen,
                Heuristic = heuristic,
                ActorList = new SortedList<GameObject, float>(),
                Locator = locator,
                AdditionalCost = additionalCost,
                HintAsGoodCell = hintAsGoodCell,
                AdditionalCostRefCount = additionalCostRefCount,
                GoodCellSessionId = goodCellSessionId,
                RefHintWeight = refHintWeight,
                ArrivalDirection = arrivalDirection,
                RefHintNode = refHintNode
            };
        }
        public static NavigationGridCell ReadVersion7(BinaryReader br, int id)
        {
            float centerHeight = br.ReadSingle();
            int sessionId = br.ReadInt32();
            float arrivalCost = br.ReadSingle();
            bool isOpen = br.ReadUInt32() == 1;
            float heuristic = br.ReadSingle();
            NavigationGridLocator locator = new NavigationGridLocator(br);
            uint actorList = br.ReadUInt32();
            br.ReadUInt32();
            int goodCellSessionId = br.ReadInt32();
            float refHintWeight = br.ReadSingle();
            br.ReadUInt16();
            short arrivalDirection = br.ReadInt16();
            short[] refHintNode = new short[] { br.ReadInt16(), br.ReadInt16() };

            return new NavigationGridCell()
            {
                ID = id,
                CenterHeight = centerHeight,
                SessionId = sessionId,
                ArrivalCost = arrivalCost,
                IsOpen = isOpen,
                Heuristic = heuristic,
                ActorList = new SortedList<GameObject, float>(),
                Locator = locator,
                GoodCellSessionId = goodCellSessionId,
                RefHintWeight = refHintWeight,
                ArrivalDirection = arrivalDirection,
                RefHintNode = refHintNode
            };
        }

        /// <summary>
        /// Sets this cell's terrain flags.
        /// </summary>
        /// <param name="flags">Flags to set.</param>
        public void SetFlags(NavigationGridCellFlags flags)
        {
            this.Flags |= flags;
        }

        /// <summary>
        /// Whether or not this cell has the specified flag(s).
        /// </summary>
        /// <param name="flag">Flag(s) to check for.</param>
        /// <returns>True/False</returns>
        public bool HasFlag(NavigationGridCellFlags flag)
        {
            return (this.Flags & flag) == flag;
        }

        /// <summary>
        /// Sets the cell as open or not for pathing.
        /// Usually false when a statistically significant amount of the cell is occupied by Actors.
        /// </summary>
        /// <param name="isOpen">True = can be pathed on. False = unable to be pathed on.</param>
        public void SetOpen(bool isOpen)
        {
            IsOpen = isOpen;
        }

        /// <summary>
        /// Gets the distance in cells from one cell to another.
        /// </summary>
        /// <param name="a">First cell.</param>
        /// <param name="b">Second cell.</param>
        /// <returns>Distance where each unit represents a cell (or close to 50 normal units).</returns>
        public static int Distance(NavigationGridCell a, NavigationGridCell b)
        {
            return NavigationGridLocator.Distance(a.Locator, b.Locator);
        }

        public Vector2 GetCenter()
        {
            return new Vector2(Locator.X + 0.5f, Locator.Y + 0.5f);
        }
    }
}
