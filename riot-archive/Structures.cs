using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace freedompeace.RiotArchive
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct RiotArchiveHeader
    {
        public uint Magic;
        public uint Version;
        public uint ManagerIndex;
        public uint FilesOffset;
        public uint PathListOffset;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct RiotPathListHeader
    {
        // Size, in bytes, of the PathList
        public uint SizeInBytes;
        // Number of path strings contained in the path list
        public uint Length;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct RiotPathListItem
    {
        // offset *from the path list* that the string begins
        public uint Offset;
        // length of the string, in bytes
        public uint Length;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct RiotFileListHeader
    {
        // Number of files in the file list
        public uint Length;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct RiotFileListEntry
    {
        public uint Hash;
        public uint DataOffset;
        public uint DataLength;
        public uint PathListIndex;
    }
}
