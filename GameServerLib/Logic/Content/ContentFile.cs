using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Content
{
    public class ContentFile
    {
        public Dictionary<string, Dictionary<string, string>> Values { get; set; }
        public Dictionary<string, object> MetaData { get; set; }

        private UInt32 Hash(string section, string name)
        {
            UInt32 hash = 0;
            foreach (var c in section)
                hash = char.ToLower(c) + 65599 * hash;
            hash = char.ToLower('*') + 65599 * hash;
            foreach (var c in name)
                hash = char.ToLower(c) + 65599 * hash;
            return hash;
        }
        public string GetObject(string section, string name)
        {
            if (Values.ContainsKey(section))
            {
                if (Values[section].ContainsKey(name))
                { 
                    return Values[section][name];
                }
            }
            if (Values.ContainsKey("UNKNOWN_HASHES"))
            {
                var hash = Hash(section, name).ToString();
                if (Values["UNKNOWN_HASHES"].ContainsKey(hash))
                {
                    //TODO: Log that unkown hash was found!
                    return Values["UNKNOWN_HASHES"][hash];
                }
            }
            return null;
        }

        public string GetString(string section, string name, string defaultValue = "")
        {
            var obj = GetObject(section, name);
            return obj == null ? defaultValue : obj;
        }

        public int GetInt(string section, string name, int defaultValue = 0)
        {
            var obj = GetObject(section, name);
            return obj == null ? defaultValue : Int32.Parse(obj);
        }

        public bool GetBool(string section, string name, bool defaultValue = false)
        {
            return GetInt(section, name, defaultValue ? 1 : 0) != 0;
        }

        public float GetFloat(string section, string name, float defaultValue = 0)
        {
            var obj = GetObject(section, name);
            return obj == null ? defaultValue : float.Parse(obj, CultureInfo.InvariantCulture);
        }

        public float[] GetArray(string section, string name, float[] defaultValue )
        {
            var obj = GetObject(section, name);
            if(obj != null)
            {
                var list = obj.Split(' ');
                if(defaultValue.Length == list.Length)
                {
                    for(int i = 0; i<defaultValue.Length; i++)
                    {
                        defaultValue[i] = float.Parse(list[i], CultureInfo.InvariantCulture);
                    }
                }
            }
            return defaultValue;
        }

        public float[] GetMultiFloat(string section, string name, int num = 6, float defaultValue = 0)
        {
            float[] result = new float[num+1];
            result[0] = GetFloat(section, name, defaultValue);
            for(int i = 1; i < num + 1; i++)
            {
                result[i] = GetFloat(section, string.Format("{0}{1}", name, i), result[0]);
            }
            return result;
        }

        public int[] GetMultiInt(string section, string name, int num = 6, int defaultValue = 0)
        {
            int[] result = new int[num + 1];
            result[0] = GetInt(section, name, defaultValue);
            for (int i = 1; i < num + 1; i++)
            {
                result[i] = GetInt(section, string.Format("{0}{1}", name, i), result[0]);
            }
            return result;
        }
    }
}
