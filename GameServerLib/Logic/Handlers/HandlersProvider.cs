using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using LeagueSandbox.GameServer.Logic.Attributes;
using LeagueSandbox.GameServer.Logic.Chatbox;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;
using Ninject;

namespace LeagueSandbox.GameServer.Logic.Handlers
{
    public class HandlersProvider : IHandlersProvider
    {
        private IKernel _kernel;

        public HandlersProvider(IKernel kernel)
        {
            _kernel = kernel;
        }

        public Dictionary<PacketCmd, Dictionary<Channel, IPacketHandler>> GetAllPacketHandlers(IEnumerable<Assembly> loadFrom)
        {
            var handlersMap = new Dictionary<PacketCmd, Dictionary<Channel, IPacketHandler>>();
            var assemblies = loadFrom?.ToList();
            if (assemblies == null)
                return handlersMap;

            foreach (var assembly in assemblies)
            {
                var possibleHandlers = assembly.GetTypes().Where(x => typeof(PacketHandlerBase).IsAssignableFrom(x));
                var handlers = possibleHandlers.Where(x => !x.IsAbstract && !x.IsInterface);
                foreach (var handler in handlers)
                {
                    // Check if handler is disabled
                    if (handler.GetCustomAttribute<DisabledHandlerAttribute>() != null)
                        continue;

                    var instance = _kernel.Get(handler) as PacketHandlerBase;
                    if (instance == null) //??
                        continue;

                    if (!handlersMap.ContainsKey(instance.PacketType))
                        handlersMap.Add(instance.PacketType, new Dictionary<Channel, IPacketHandler>());
                    if (handlersMap[instance.PacketType].ContainsKey(instance.PacketChannel))
                        throw new InvalidOperationException($"Handler for packet {instance.PacketType} " +
                                                            $"and channel {instance.PacketChannel} already exists.");

                    handlersMap[instance.PacketType].Add(instance.PacketChannel, instance);
                }
            }

            return handlersMap;
        }

        public SortedDictionary<string, IChatCommand> GetAllChatCommandHandlers(IEnumerable<Assembly> loadFrom)
        {
            var handlersMap = new SortedDictionary<string, IChatCommand>();
            var assemblies = loadFrom?.ToList();
            if (assemblies == null)
                return handlersMap;

            foreach (var assembly in assemblies)
            {
                var possibleHandlers = assembly.GetTypes().Where(x => typeof(IChatCommand).IsAssignableFrom(x));
                var handlers = possibleHandlers.Where(x => !x.IsAbstract && !x.IsInterface);
                foreach (var handler in handlers)
                {
                    // Check if handler is disabled
                    if (handler.GetCustomAttribute<DisabledHandlerAttribute>() != null)
                        continue;

                    var instance = _kernel.Get(handler) as IChatCommand;
                    if (instance == null) //??
                        continue;
                    if (handlersMap.ContainsKey(instance.Command))
                        throw new InvalidOperationException($"Handler for command {instance.Command}" +
                                                            $" already exists. " +
                                                            $"({handlersMap[instance.Command]}/{instance})");

                    handlersMap.Add(instance.Command, instance);
                }
            }

            return handlersMap;
        }
    }
}
