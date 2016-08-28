using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Chatbox;
using NLua.Exceptions;
using LeagueSandbox.GameServer.Logic.Scripting;
using LeagueSandbox.GameServer.Logic.Scripting.Lua;

namespace LeagueSandbox.GameServer.Logic.API
{
    class ScriptedChatCommand : ChatCommand
    {
        private IScriptEngine _scriptEngine;

        public void RegisterCommand(string command, string syntax)
        {
            Command = command;
            Syntax = syntax;
        }

        public ScriptedChatCommand(string location, ChatboxManager owner) : this("", "", owner)
        {
            _scriptEngine = new LuaScriptEngine();
            _scriptEngine.RegisterFunction("registerCommand", this, typeof(ScriptedChatCommand).GetMethod("RegisterCommand", new Type[] { typeof(string), typeof(string) }));
            _scriptEngine.Load(location);
            ApiFunctionManager.AddBaseFunctionToLuaScript(_scriptEngine);
        }

        public ScriptedChatCommand(string command, string syntax, ChatboxManager owner) : base(command, syntax, owner)
        {
        }

        public override void Execute(Peer peer, bool hasReceivedArguments, string arguments = "")
        {
            var split = arguments.ToLower().Split(' ');
            try
            {
                _scriptEngine.SetGlobalVariable("args", split);
                _scriptEngine.Execute("onExecute()");
            }
            catch (LuaScriptException e)
            {
                Logger.LogCoreError("LUA ERROR : " + e.Message);
            }
        }
    }
}
