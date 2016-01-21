using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace complete.IO
{
    static class BinaryReaderExtensions
    {
        public static unsafe T ReadStruct<T>(this BinaryReader reader)
        {
            var type = typeof(T);
            var size = Marshal.SizeOf(type);
            var bytes = reader.ReadBytes(size);

            fixed (byte* b = bytes)
                return (T)Marshal.PtrToStructure(new IntPtr(b), type);
        }
    }
}
