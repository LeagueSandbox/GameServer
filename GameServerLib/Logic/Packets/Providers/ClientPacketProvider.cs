using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;
using Ninject;

namespace LeagueSandbox.GameServer.Logic.Packets.Providers
{
    public class ClientPacketProvider : IClientPacketProvider
    {
        private Logger _logger;

        public ClientPacketProvider(Logger logger)
        {
            _logger = logger;
        }

        public ClientPacketBase ProvideClientPacket(IPacketHandler handler, byte[] data)
        {
            if (handler == null)
                return null;

            var argumentType = GetGenericBaseArgumentType(handler);
            if (argumentType == null)
            {
                _logger.LogFatalError($"No generic argument of type {nameof(ClientPacketBase)} found " +
                                      $"for client packet handler {handler}!");
                return null;
            }

            return (ClientPacketBase)Activator.CreateInstance(argumentType, data);
        }

        private static Type GetGenericBaseArgumentType(IPacketHandler handler)
        {
            //TODO: add Type cache to avoid performance issues
            var type = handler.GetType();
            while (type != null)
            {
                var predicate = new Func<Type, bool>(x => typeof(ClientPacketBase).IsAssignableFrom(x));
                var genericArgs = type.GetGenericArguments().Where(predicate).ToList();
                if (genericArgs.Any())
                    return genericArgs.First();

                type = type.BaseType;
            }

            return null;
        }
    }
}
