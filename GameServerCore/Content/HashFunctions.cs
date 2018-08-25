namespace GameServerCore.Content
{
    public class HashFunctions
    {
        public static uint HashString(string path)
        {
            uint hash = 0;
            var mask = 0xF0000000;
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

        public static uint HashStringSdbm(string section, string name)
        {
            uint hash = 0;
            foreach (var c in section)
            {
                hash = char.ToLower(c) + 65599 * hash;
            }

            hash = char.ToLower('*') + 65599 * hash;
            foreach (var c in name)
            {
                hash = char.ToLower(c) + 65599 * hash;
            }

            return hash;
        }
    }


}
