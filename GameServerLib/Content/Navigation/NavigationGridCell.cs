using System;
using System.IO;

namespace LeagueSandbox.GameServer.Content.Navigation
{
    public class NavigationGridCell
    {
        public NavigationGridCellFlags Flags { get; private set; }
        public int ID { get; private set; }
        public float CenterHeight { get; private set; }
        public uint SessionId { get; private set; }
        public float ArrivalCost { get; private set; }
        public bool IsOpen { get; private set; }
        public float Heuristic { get; private set; }
        public uint ActorList { get; private set; }
        public short X { get; private set; }
        public short Y { get; private set; }
        public float AdditionalCost { get; private set; }
        public float HintAsGoodCell { get; private set; }
        public uint AdditionalCostRefCount { get; private set; }
        public uint GoodCellSessionId { get; private set; }
        public float RefHintWeight { get; private set; }
        public short ArrivalDirection { get; private set; }
        public short[] RefHintNode { get; private set; } = new short[2];

        private NavigationGridCell() { }

        public static NavigationGridCell ReadVersion5(BinaryReader br, int id)
        {
            float centerHeight = br.ReadSingle();
            uint sessionId = br.ReadUInt32();
            float arrivalCost = br.ReadSingle();
            bool isOpen = br.ReadUInt32() == 1;
            float heuristic = br.ReadSingle();
            uint actorList = br.ReadUInt32();
            short x = br.ReadInt16();
            short y = br.ReadInt16();
            float additionalCost = br.ReadSingle();
            float hintAsGoodCell = br.ReadSingle();
            uint additionalCostRefCount = br.ReadUInt32();
            uint goodCellSessionId = br.ReadUInt32();
            float refHintWeight = br.ReadSingle();
            short arrivalDirection = br.ReadInt16();
            NavigationGridCellFlags flags = (NavigationGridCellFlags)br.ReadUInt16();
            short refHintNode1 = br.ReadInt16();
            short refHintNode2 = br.ReadInt16();

            return new NavigationGridCell()
            {
               ID = id,
               Flags = flags,
               CenterHeight = centerHeight,
               SessionId = sessionId,
               ArrivalCost = arrivalCost,
               IsOpen = isOpen,
               Heuristic = heuristic,
               ActorList = actorList,
               X = x,
               Y = y,
               AdditionalCost = additionalCost,
               HintAsGoodCell = hintAsGoodCell,
               AdditionalCostRefCount = additionalCostRefCount,
               GoodCellSessionId = goodCellSessionId,
               RefHintWeight = refHintWeight,
               ArrivalDirection = arrivalDirection,
               RefHintNode = new short[] { refHintNode1, refHintNode2 }
           };
        }
        public static NavigationGridCell ReadVersion7(BinaryReader br, int id)
        {
            float centerHeight = br.ReadSingle();
            uint sessionId = br.ReadUInt32();
            float arrivalCost = br.ReadSingle();
            bool isOpen = br.ReadUInt32() == 1;
            float heuristic = br.ReadSingle();
            short x = br.ReadInt16();
            short y = br.ReadInt16();
            uint actorList = br.ReadUInt32();
            br.ReadUInt32();
            uint goodCellSessionId = br.ReadUInt32();
            float refHintWeight = br.ReadSingle();
            br.ReadUInt16();
            short arrivalDirection = br.ReadInt16();
            short refHintNode1 = br.ReadInt16();
            short refHintNode2 = br.ReadInt16();

            return new NavigationGridCell()
            {
                ID = id,
                CenterHeight = centerHeight,
                SessionId = sessionId,
                ArrivalCost = arrivalCost,
                IsOpen = isOpen,
                Heuristic = heuristic,
                ActorList = actorList,
                X = x,
                Y = y,
                GoodCellSessionId = goodCellSessionId,
                RefHintWeight = refHintWeight,
                ArrivalDirection = arrivalDirection,
                RefHintNode = new short[] { refHintNode1, refHintNode2 }
            };
        }

        public void SetFlags(NavigationGridCellFlags flags)
        {
            this.Flags |= flags;
        }
        public bool HasFlag(NavigationGridCellFlags flag)
        {
            return (this.Flags & flag) == flag;
        }

        public static int Distance(NavigationGridCell a, NavigationGridCell b)
        {
            return (Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y));
        }
    }

    public enum NavigationGridCellFlags : ushort
    {
        HAS_GRASS = 0x1,
        NOT_PASSABLE = 0x2,
        BUSY = 0x4,
        TARGETTED = 0x8,
        MARKED = 0x10,
        PATHED_ON = 0x20,
        SEE_THROUGH = 0x40,
        OTHER_DIRECTION_END_TO_START = 0x80,
        HAS_GLOBAL_VISION = 0x100,
        // HAS_TRANSPARENT_TERRAIN = 0x42 // (SeeThrough | NotPassable)
    }
}
