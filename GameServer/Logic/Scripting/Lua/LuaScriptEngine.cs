using NLua;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace LeagueSandbox.GameServer.Logic.Scripting.Lua
{
    public class LuaScriptEngine : IScriptEngine
    {
        private bool _isLoaded;
        private NLua.Lua _lua;

        public LuaScriptEngine()
        {
            _lua = new NLua.Lua();
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
                var s = _lua.DoFile(location);
                _isLoaded = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void RegisterFunction(string path, object target, MethodBase function)
        {
            _lua.RegisterFunction(path, target, function);
        }

        public void Execute(string script)
        {
            _lua.DoString(script);
        }

        public void SetGlobalVariable(string name, object value)
        {
            _lua[name] = value;
        }

        private LuaTable getTable(string name)
        {
            if (!_isLoaded)
                return null;

            return _lua.GetTable(name);
        }

        private Dictionary<object, object> getTableDictionary(string name)
        {
            if (!_isLoaded)
                return null;

            return _lua.GetTableDict(getTable(name));
        }

        private Dictionary<object, object> getTableDictionary(LuaTable table)
        {
            if (!_isLoaded)
                return null;

            return _lua.GetTableDict(table);
        }

        //public void setFunction();

        /*void addChampion();
        void addUnit();
        void addItem();
        void addGame();*/
    }
}
