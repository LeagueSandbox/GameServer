using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.API
{
    public static class ApiEventManager
    {
        private static Game _game;
        private static Logger _logger;

        internal static void SetGame(Game game)
        {
            _game = game;
            _logger = Program.ResolveDependency<Logger>();
        }

        public static void removeAllListenersForOwner(Object owner)
        {
            removeListenerOnChampionDamaged(owner);
        }
        
        // ************ OnChampionDamaged Event *************
        private static List<Tuple<Object, Champion, Action>> onChampionDamagedListeners = new List<Tuple<object, Champion, Action>>();
        public static void addListenerOnChampionDamaged(Object owner, Champion champion, Action callback)
        {
            var listenerTuple = new Tuple<object, Champion, Action>(owner, champion, callback);
            onChampionDamagedListeners.Add(listenerTuple);
        }
        public static void removeListenerOnChampionDamaged(Object owner, Champion champion)
        {
            onChampionDamagedListeners.RemoveAll((listener) => listener.Item1 == owner && listener.Item2 == champion);
        }
        public static void removeListenerOnChampionDamaged(Object owner)
        {
            onChampionDamagedListeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public static void publishOnChampionDamaged(Champion champion)
        {
            onChampionDamagedListeners.ForEach((listener)=> {
                if (listener.Item2 == champion) {
                    listener.Item3(); }
            });
        }
    }
}