using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace complete.IO
{
    static class BinaryReaderTools
    {
        // ascii string
        internal static string GetStaticLengthString(BinaryReader reader, int length)
        {
            var paddedNameBytes = reader.ReadBytes(length);
            var paddedName = Encoding.ASCII.GetChars(paddedNameBytes);

            // trim the null-terminator
            var stringLength = paddedName.Length;
            for (var i = 0; i < stringLength; i++)
            {
                if (paddedName[i] == 0)
                {
                    stringLength = i;
                    break;
                }
            }

            return new string(paddedName, 0, stringLength);
        }

        // ascii string
        internal static string GetString(BinaryReader reader)
        {
            var stringBuilder = new StringBuilder();
            byte c;
            while ((c = reader.ReadByte()) != 0)
                stringBuilder.Append((char)c);
            return stringBuilder.ToString();
        }
        // ascii string. returns number of bytes read.
        internal static int GetString(BinaryReader reader, out string @string)
        {
            var stringBuilder = new StringBuilder();
            var bytesRead = 0;
            while (true)
            {
                bytesRead++;
                var c = reader.ReadByte();
                if (c == 0)
                    break;
                stringBuilder.Append((char)c);
            }
            @string = stringBuilder.ToString();
            return bytesRead;
        }
    }
}
