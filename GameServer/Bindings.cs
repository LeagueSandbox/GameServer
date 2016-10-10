using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.RAF;
using LeagueSandbox.GameServer.Logic.Chatbox;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Players;
using LeagueSandbox.GameServer.Logic.Scripting;
using LeagueSandbox.GameServer.Logic.Scripting.Lua;
using Ninject.Modules;

namespace LeagueSandbox.GameServer
{
    class Bindings : NinjectModule
    {
        public override void Load()
        {
            // Singletons - Only one instance of these objects will ever be created.
            Bind<Server>().To<Server>().InSingletonScope();

            Bind<Logger>().To<Logger>().InSingletonScope();
            Bind<ServerContext>().To<ServerContext>().InSingletonScope();
            Bind<RAFManager>().To<RAFManager>().InSingletonScope();
            Bind<Game>().To<Game>().InSingletonScope();

            Bind<ItemManager>().To<ItemManager>().InSingletonScope();
            Bind<ChatboxManager>().To<ChatboxManager>().InSingletonScope();
            Bind<PlayerManager>().To<PlayerManager>().InSingletonScope();
            Bind<NetworkIdManager>().To<NetworkIdManager>().InSingletonScope();

            // Other bindings
            Bind<IScriptEngine>().To<LuaScriptEngine>();
        }
    }
}