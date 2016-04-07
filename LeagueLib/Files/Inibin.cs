using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LeagueLib.Files
{
    public class Inibin
    {
        public string FilePath { get; internal set; }

        public byte Version { get; internal set; }
        public Dictionary<uint, InibinValue> Content { get; internal set; }

        public Inibin() { }

        public static Inibin DeserializeInibin(byte[] data, string filepath)
        {
            var reader = new InibinReader();
            return reader.DeserializeInibin(data, filepath);
        }
    }

    public class InibinValue
    {
        public int Type { get; set; }
        public object Value { get; set; }

        public InibinValue(int type, object value)
        {
            Type = type;
            Value = value;
        }
    }
}
