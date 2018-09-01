using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace LeagueSandbox.GameServer
{
    public static class ServerContext
    {
        public static string ExecutingDirectory { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        public static string BuildDateString =>
            BuildDate.GetBuildDateTime().ToString("G", CultureInfo.InvariantCulture) + " UTC";
    }

    public static class BuildDate
    {
        private struct ImageFileHeader
        {
            #pragma warning disable 0649
            public ushort Machine;
            public ushort NumberOfSections;
            public uint TimeDateStamp;
            public uint PointerToSymbolTable;
            public uint NumberOfSymbols;
            public ushort SizeOfOptionalHeader;
            public ushort Characteristics;
        }

        public static DateTime GetBuildDateTime()
        {
            var path = Path.Combine(ServerContext.ExecutingDirectory, "GameServerLib.dll");
            if (File.Exists(path))
            {
                var buffer = new byte[Math.Max(Marshal.SizeOf(typeof(ImageFileHeader)), 4)];
                using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    fs.Position = 0x3C;
                    fs.Read(buffer, 0, 4);
                    fs.Position = BitConverter.ToUInt32(buffer, 0); // COFF header offset
                    fs.Read(buffer, 0, 4); // "PE\0\0"
                    fs.Read(buffer, 0, buffer.Length);
                }

                var pinnedBuffer = GCHandle.Alloc(buffer, GCHandleType.Pinned);

                try
                {
                    var coffHeader = (ImageFileHeader)Marshal.PtrToStructure(pinnedBuffer.AddrOfPinnedObject(),
                        typeof(ImageFileHeader));

                    return new DateTime(1970, 1, 1).AddSeconds(coffHeader.TimeDateStamp);
                }
                finally
                {
                    pinnedBuffer.Free();
                }
            }

            return new DateTime(2014, 11, 20);
        }
    }
}
