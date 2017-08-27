using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Chatbox;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.Handlers;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Packets.PacketArgs;
using LeagueSandbox.GameServer.Logic.Packets.Providers;
using LeagueSandbox.GameServer.Logic.Players;
using LeagueSandbox.GameServer.Logic.Scripting.CSharp;
using Ninject.Modules;

namespace LeagueSandbox.GameServer
{
    public class Bindings : NinjectModule
    {
        public override void Load()
        {
            // Singletons - Only one instance of these objects will ever be created.
            Bind<Server>().To<Server>().InSingletonScope();

            Bind<Logger>().To<Logger>().InSingletonScope();
            Bind<ServerContext>().To<ServerContext>().InSingletonScope();
            Bind<Game>().To<Game>().InSingletonScope();

            Bind<ItemManager>().To<ItemManager>().InSingletonScope();
            Bind<ChatCommandManager>().To<ChatCommandManager>().InSingletonScope();
            Bind<PlayerManager>().To<PlayerManager>().InSingletonScope();
            Bind<NetworkIdManager>().To<NetworkIdManager>().InSingletonScope();

            Bind<CSharpScriptEngine>().To<CSharpScriptEngine>().InSingletonScope();
            Bind<IHandlersProvider>().To<HandlersProvider>();
            Bind<IClientPacketProvider>().To<ClientPacketProvider>();
            Bind<IPacketArgsTranslationService>().To<PacketArgsTranslationService>();
        }
    }
}