using System;
using System.Collections.Generic;

namespace GameServerCore.Packets.Handlers
{
    // the global generic network handler between the bridge and the server
    public class NetworkHandler<MessageType>
    {
        public delegate bool MessageHandler<T>(int userId, T msg) where T : MessageType;
        private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();

        // server & scripts will register to events, this allows scripts more than the current state
        public void Register<T>(MessageHandler<T> handler) where T: MessageType
        {
            if (handler == null) return;
            _handlers[typeof(T)].Add(handler);
        }
        // every message (bridge->server or server->bridge) pass should pass here
        public bool OnMessage<T>(int userId, T req) where T: MessageType
        {
            return Handle<T>(userId, req);
        }
        private bool Handle<T>(int userId, T message) where T: MessageType
        {
            List<MessageHandler<T>> handlerList = _handlers[typeof(T)] as List<MessageHandler<T>>;
            bool success = true;
            foreach(MessageHandler<T> handler in handlerList)
            {
                success = handler(userId, message) && success;
            }
            return success;
        }
    }
}
