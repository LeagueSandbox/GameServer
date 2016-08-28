using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Chatbox;
using NLua.Exceptions;

namespace LeagueSandbox.GameServer.Logic.API
{
    class LuaChatCommand : ChatCommand
    {
        private LuaScript _luaScript;

        public void RegisterCommand(string command, string syntax)
        {
            Command = command;
            Syntax = syntax;
        }

        public LuaChatCommand(string location, ChatboxManager owner) : this("", "", owner)
        {
            _luaScript = new LuaScript();
            _luaScript.lua.RegisterFunction("registerCommand", this, typeof(LuaChatCommand).GetMethod("RegisterCommand", new Type[] { typeof(string), typeof(string) }));
            _luaScript.loadScript(location);
            ApiFunctionManager.AddBaseFunctionToLuaScript(_luaScript);
        }

        public LuaChatCommand(string command, string syntax, ChatboxManager owner) : base(command, syntax, owner)
        {
        }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            try
            {
                _luaScript.lua["args"] = split;
                _luaScript.lua.DoString("onExecute()");
            }
            catch (LuaScriptException e)
            {
                Logger.LogCoreError("LUA ERROR : " + e.Message);
            }
        }
    }
}
