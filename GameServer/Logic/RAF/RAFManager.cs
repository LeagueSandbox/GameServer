using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace LeagueSandbox.GameServer.Core.Logic.RAF
{
    static class RAFManager
    {
        private static Game _game = Program.ResolveDependency<Game>();
        public static uint GetHash(string path)
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

        public static bool ReadSpellData(string spellName, out JObject data)
        {
            try
            {
                var path = _game.Config.ContentManager.GetSpellDataPath(spellName);
                data = JObject.Parse(File.ReadAllText(path));

                return true;
            }
            catch
            {
                data = new JObject();
                return false;
            }
        }

        public static bool ReadMissileData(string spellName, out JObject data)
        {
            if (!ReadSpellData(spellName + "Missile", out data))
            {
                return ReadSpellData(spellName + "Mis", out data);
            }

            return ReadSpellData(spellName + "Missile", out data);
        }

        public static bool ReadAutoAttackData(string unitName, out JObject data)
        {
            return ReadSpellData(unitName + "BasicAttack", out data);
        }

        public static bool ReadUnitStats(string unitName, out JObject data)
        {
            var path = _game.Config.ContentManager.GetUnitStatPath(unitName);
            data = JObject.Parse(File.ReadAllText(path));

            return true;
        }

        public static JToken GetValue(JObject from, string first, string second = "", string third = "")
        {
            var toReturn = from.SelectToken(first);
            if (second != "")
            {
                toReturn = toReturn.SelectToken(second);
            }
            if (third != "")
            {
                toReturn = toReturn.SelectToken(third);
            }

            return toReturn;
        }

        public static float GetFloatValue(JObject from, string first, string second = "", string third = "")
        {
            var x = GetValue(from, first, second, third);
            return x == null ? 0.0f : (float)x;
        }

        public static int GetIntValue(JObject from, string first, string second = "", string third = "")
        {
            var x = GetValue(from, first, second, third);
            return x == null ? 0 : (int)x;
        }

        public static string GetStringValue(JObject from, string first, string second = "", string third = "")
        {
            var x = GetValue(from, first, second, third);
            return x == null ? "" : (string)x;
        }

        public static bool GetBoolValue(JObject from, string first, string second = "", string third = "")
        {
            var x = GetValue(from, first, second, third);
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
