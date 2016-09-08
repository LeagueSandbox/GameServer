using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.API;
using LeagueSandbox.GameServer.Logic.Scripting;
using LeagueSandbox.GameServer.Logic.Scripting.Lua;
using NLua.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class Talent
    {
        private Logger _logger = Program.ResolveDependency<Logger>();
        protected IScriptEngine _scriptEngine = new LuaScriptEngine();

        protected string _name;
        protected int _rank;
        protected Game _game;
        protected Unit _owner;

        public Unit GetOwner()
        {
            return _owner;
        }

        public string GetName()
        {
            return _name;
        }

        public int GetRank()
        {
            return _rank;
        }

        public Talent(Game game, string talentName, int rank, Unit owner)
        {
            _game = game;
            _name = talentName;
            _owner = owner;
            LoadLua();
            try
            {
                _scriptEngine.Execute("onAddTalent()");
            }
            catch (LuaException e)
            {
                _logger.LogCoreError("LUA ERROR : " + e.Message);
            }
        }


        public void LoadLua()
        {
            var scriptLoc = _game.Config.ContentManager.GetTalentScriptPath(_name);
            _logger.LogCoreInfo("Loading talent from " + scriptLoc);

            _scriptEngine.Execute("package.path = 'LuaLib/?.lua;' .. package.path");
            _scriptEngine.Execute(@"
                function onAddTalent()
                end");
            _scriptEngine.Execute(@"
                function onUpdate(diff)
                end");
            _scriptEngine.RegisterFunction("getOwner", this, typeof(Buff).GetMethod("GetOwner"));
            _scriptEngine.RegisterFunction("getRank", this, typeof(Buff).GetMethod("GetRank"));

            ApiFunctionManager.AddBaseFunctionToLuaScript(_scriptEngine);

            _scriptEngine.Load(scriptLoc);
        }


        public void Update(long diff)
        {
            if (_scriptEngine.IsLoaded())
            {
                try
                {
                    _scriptEngine.SetGlobalVariable("diff", diff);
                    _scriptEngine.Execute("onUpdate(diff)");
                }
                catch (LuaException e)
                {
                    _logger.LogCoreError("LUA ERROR : " + e.Message);
                }
            }
        }

    }
}
