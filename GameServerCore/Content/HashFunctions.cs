namespace GameServerCore.Content
{
    public class HashFunctions
    {
        public static uint HashString(string path)
        {
            uint hash = 0;
            const uint mask = 0xF0000000;
            for (var i = 0; i < path.Length; i++)
            {
                hash = char.ToLower(path[i]) + 0x10 * hash;
                if ((hash & mask) > 0)
                {
                    hash ^= hash & mask ^ (hash & mask) >> 24;
                }
            }

            return hash;
        }

        public static uint HashStringSdbm(string data)
        {
            uint hash = 0;
            foreach (var c in data)
            {
                hash = char.ToLower(c) + 65599 * hash;
            }

            return hash;
        }

        public static uint HashStringSdbm(string section, string name)
        {
            return HashStringSdbm($"{section}*{name}");
        }
    }


}
