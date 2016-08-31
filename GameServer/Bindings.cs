using LeagueSandbox.GameServer.Core.Logic;
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
            
            // Singletons - Only one instance of these objects will ever be created.
            Bind<ServerContext>().To<ServerContext>().InSingletonScope();
            Bind<Logger>().To<Logger>().InSingletonScope();
            Bind<RAFManager>().To<RAFManager>().InSingletonScope();

            Bind<IScriptEngine>().To<LuaScriptEngine>();
            
        }
    }
}
