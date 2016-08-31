using LeagueSandbox.GameServer.Core.Logic.RAF;
using LeagueSandbox.GameServer.Logic.Scripting;
using LeagueSandbox.GameServer.Logic.Scripting.Lua;
using Ninject.Modules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer
{
    class Bindings : NinjectModule
    {
        public override void Load()
        {
            Bind<Server>().To<Server>();
            
            Bind<ServerContext>().To<ServerContext>().InSingletonScope();
            Bind<RAFManager>().ToMethod(context => RAFManager.getInstance()).InSingletonScope();

            Bind<IScriptEngine>().To<LuaScriptEngine>();
            
        }
    }
}
