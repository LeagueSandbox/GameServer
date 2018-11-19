using GameServerCore.Packets.PacketDefinitions;
using System;
using System.Collections.Generic;

namespace GameServerCore.Packets
{
    // the global generic network handler between the bridge and the server
    class NetworkHandler
    {
        public delegate void MessageHandler<T>(T msg) where T : ICoreMessage;
        private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();

        // server & scripts will register to events, this allows scripts more than the current state
        public void Register<T>(MessageHandler<T> handler) where T: ICoreMessage
        {
            if (handler == null) return;
            _handlers[typeof(T)].Add(handler);
        }
        // every message (bridge->server or server->bridge) pass should pass here
        public void OnMessage<T>(T req) where T: ICoreMessage
        {
            Handle<T>(req);
        }
        private void Handle<T>(T message) where T: ICoreMessage
        {
            List<MessageHandler<T>> handlerList = _handlers[typeof(T)] as List<MessageHandler<T>>;
            foreach(MessageHandler<T> handler in handlerList)
            {
                handler(message);
            }
        }
    }
}
