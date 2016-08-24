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
        private bool _isLoaded;
        public Lua Lua;

        public LuaScript()
        {
            Lua = new Lua();
        }

        public bool IsLoaded()
        {
            return _isLoaded;
        }

        public void Load(string location)
        {
            _isLoaded = false;
            try
            {
                var s = Lua.DoFile(location);
                _isLoaded = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private LuaTable getTable(string name)
        {
            if (!_isLoaded)
                return null;

            return Lua.GetTable(name);
        }

        private Dictionary<object, object> getTableDictionary(string name)
        {
            if (!_isLoaded)
                return null;

            return Lua.GetTableDict(getTable(name));
        }

        private Dictionary<object, object> getTableDictionary(LuaTable table)
        {
            if (!_isLoaded)
                return null;

            return Lua.GetTableDict(table);
        }

        //public void setFunction();

        /*void addChampion();
        void addUnit();
        void addItem();
        void addGame();*/
    }
}
