using NLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic
{
    public class LuaScript
    {
        private bool loaded = false;
        public Lua lua;

        public LuaScript()
        {
            lua = new Lua();
        }
        public bool isLoaded()
        {
            return loaded;
        }
        public void loadScript(string location)
        {
            loaded = false;
            try
            {
                var s = lua.DoFile(location);
                loaded = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public LuaTable getTable(string name)
        {
            if (!loaded)
                return null;

            return lua.GetTable(name);
        }

        public Dictionary<object, object> getTableDictionary(string name)
        {
            if (!loaded)
                return null;

            return lua.GetTableDict(getTable(name));
        }

        public Dictionary<object, object> getTableDictionary(LuaTable table)
        {
            if (!loaded)
                return null;

            return lua.GetTableDict(table);
        }

        //public void setFunction();
        public void setLoaded(bool load)
        {
            loaded = load;
        }

        /*void addChampion();
        void addUnit();
        void addItem();
        void addGame();*/
    }
}
