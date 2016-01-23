using NLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntWarsSharp.Logic
{
    public class LuaScript
    {
        private bool loaded = false;
        private Lua lua;

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
            lua.LoadFile(location);
            loaded = true;
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
