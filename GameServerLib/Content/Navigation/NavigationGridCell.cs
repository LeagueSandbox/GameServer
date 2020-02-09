using System;
using System.IO;

namespace LeagueSandbox.GameServer.Content.Navigation
{
    public class NavigationGridCell
    {
        public int ID { private set; get; }
        public float CenterHeight { private set; get; }
        public uint SessionId { private set; get; }
        public float ArrivalCost { private set; get; }
        public bool IsOpen { private set; get; }
        public float Heuristic { private set; get; }
        public uint ActorList { private set; get; }
        public short X { private set; get; }
        public short Y { private set; get; }
        public float AdditionalCost { private set; get; }
        public float HintAsGoodCell { private set; get; }
        public uint AdditionalCostRefCount { private set; get; }
        public uint GoodCellSessionId { private set; get; }
        public float RefHintWeight { private set; get; }
        public short ArrivalDirection { private set; get; }
        public short[] RefHintNode { private set; get; } = new short[2];

        public NavigationGridCell(BinaryReader br, int id, out ushort flag)
        {
            this.ID = id;
            this.CenterHeight = br.ReadSingle();
            this.SessionId = br.ReadUInt32();
            this.ArrivalCost = br.ReadSingle();
            this.IsOpen = br.ReadUInt32() == 1;
            this.Heuristic = br.ReadSingle();
            this.ActorList = br.ReadUInt32();
            this.X = br.ReadInt16();
            this.Y = br.ReadInt16();
            this.AdditionalCost = br.ReadSingle();
            this.HintAsGoodCell = br.ReadSingle();
            this.AdditionalCostRefCount = br.ReadUInt32();
            this.GoodCellSessionId = br.ReadUInt32();
            this.RefHintWeight = br.ReadSingle();
            this.ArrivalDirection = br.ReadInt16();
            flag = br.ReadUInt16();
            this.RefHintNode[0] = br.ReadInt16();
            this.RefHintNode[1] = br.ReadInt16();
        }
        public NavigationGridCell(BinaryReader br, int id)
        {
            this.ID = id;
            this.CenterHeight = br.ReadSingle();
            this.SessionId = br.ReadUInt32();
            this.ArrivalCost = br.ReadSingle();
            this.IsOpen = br.ReadUInt32() == 1;
            this.Heuristic = br.ReadSingle();
            this.X = br.ReadInt16();
            this.Y = br.ReadInt16();
            this.ActorList = br.ReadUInt32();
            br.ReadUInt32(); // <- "Unk1"
            this.GoodCellSessionId = br.ReadUInt32();
            this.RefHintWeight = br.ReadSingle();
            br.ReadUInt16();   // <- "Unk2"
            this.ArrivalDirection = br.ReadInt16();
            this.RefHintNode[0] = br.ReadInt16();
            this.RefHintNode[1] = br.ReadInt16();
        }

        public bool HasFlag(NavigationGrid grid, NavigationGridCellFlags flag)
        {
            return grid.HasFlag(this.ID, flag);
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
