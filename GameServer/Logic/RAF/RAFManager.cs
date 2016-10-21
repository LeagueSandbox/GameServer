using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace LeagueSandbox.GameServer.Core.Logic.RAF
{
    public class RAFManager
    {
        private static Game _game = Program.ResolveDependency<Game>();
        public uint GetHash(string path)
        {
            uint hash = 0;
            var mask = 0xF0000000;
            for (var i = 0; i < path.Length; i++)
            {
                hash = char.ToLower(path[i]) + 0x10 * hash;
                if ((hash & mask) > 0)
                {
                    hash ^= hash & mask ^ ((hash & mask) >> 24);
                }
            }

            return hash;
        }

        public bool ReadSpellData(string spellName, out JObject data)
        {
            var path = _game.Config.ContentManager.GetSpellDataPath(spellName);

            data = JObject.Parse(File.ReadAllText(path));
            return true;
        }

        public bool ReadAutoAttackData(string unitName, out JObject data)
        {
            return ReadSpellData(unitName + "BasicAttack", out data);
        }

        public bool ReadUnitStats(string unitName, out JObject data)
        {
            var path = _game.Config.ContentManager.GetUnitStatPath(unitName);

            data = JObject.Parse(File.ReadAllText(path));
            return true;
        }

        public JToken GetValue(JObject from, string second = "", string third = "")
        {
            var toReturn = from.SelectToken("Values");
            if (!string.IsNullOrEmpty(second) && toReturn != null)
            {
                toReturn = toReturn.SelectToken(second);
            }
            if (!string.IsNullOrEmpty(third) && toReturn != null)
            {
                toReturn = toReturn.SelectToken(third);
            }

            return toReturn;
        }

        public float GetFloatValue(JObject from, string second = "", string third = "")
        {
            var x = GetValue(from, second, third);
            return x == null || string.IsNullOrEmpty((string)x) ? 0.0f : (float)x;
        }

        public int GetIntValue(JObject from, string second = "", string third = "")
        {
            var x = GetValue(from, second, third);
            return x == null || string.IsNullOrEmpty((string)x) ? 0 : (int)x;
        }

        public string GetStringValue(JObject from, string second = "", string third = "")
        {
            var x = GetValue(from, second, third);
            return x == null || string.IsNullOrEmpty((string)x) ? "" : (string)x;
        }

        public bool GetBoolValue(JObject from, string second = "", string third = "")
        {
            var x = GetValue(from, second, third);
            if (x == null)
            {
                return false;
            }

            var asInt = 0;
            var asBool = false;
            var asString = "No";

            try
            {
                asInt = Convert.ToInt32(x);
            }
            catch { }

            try
            {
                asBool = Convert.ToBoolean(x);
            }
            catch { }

            try
            {
                asString = Convert.ToString(x);
            }
            catch { }

            return asInt == 1 || asBool || asString == "Yes";
        }
    }
}
