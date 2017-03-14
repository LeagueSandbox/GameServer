using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*
 * Possible Events:
[OnActivate]
[OnAddPAR]
[OnAllowAdd]
[OnAssist]
[OnAssistUnit]
[OnBeingDodged]
[OnBeingHit]
[OnBeingSpellHit]
[OnCollision]
[OnCollisionTerrain]
[OnDeactivate]
[OnDealDamage]
[OnDeath]
[OnDodge]
[OnHeal]
[OnHitUnit]
[OnKill]
[OnKillUnit]
[OnLaunchAttack]
[OnLaunchMissile]
[OnLevelUp]
[OnLevelUpSpell]
[OnMiss]
[OnMissileEnd]
[OnMissileUpdate]
[OnMoveEnd]
[OnMoveFailure]
[OnMoveSuccess]
[OnNearbyDeath]
[OnPreAttack]
[OnPreDamage]
[OnPreDealDamage]
[OnPreMitigationDamage]
[OnResurrect]
[OnSpellCast]
[OnSpellHit]
[OnTakeDamage]
[OnUpdateActions]
[OnUpdateAmmo]
[OnUpdateStats]
[OnZombie]
 */

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

        public static void removeAllListenersForOwner(object owner)
        {
            OnChampionDamageTaken.RemoveListener(owner);
            OnUpdate.RemoveListener(owner);
        }
        
        public static EventOnUpdate OnUpdate = new EventOnUpdate();
        public static EventOnChampionDamageTaken OnChampionDamageTaken = new EventOnChampionDamageTaken();
        public static EventOnUnitDamageTaken OnUnitDamageTaken = new EventOnUnitDamageTaken();
    }


    public class EventOnUpdate
    {
        private List<Tuple<object, Action<float>>> listeners = new List<Tuple<object, Action<float>>>();
        public void AddListener(object owner, Action<float> callback)
        {
            var listenerTuple = new Tuple<object, Action<float>>(owner, callback);
            listeners.Add(listenerTuple);
        }

        public void RemoveListener(object owner)
        {
            listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public void Publish(float diff)
        {
            listeners.ForEach((listener) => {
                listener.Item2(diff);
            });
        }
    }

    public class EventOnUnitDamageTaken
    {
        private List<Tuple<object, Unit, Action>> listeners = new List<Tuple<object, Unit, Action>>();
        public void AddListener(object owner, Unit unit, Action callback)
        {
            var listenerTuple = new Tuple<object, Unit, Action>(owner, unit, callback);
            listeners.Add(listenerTuple);
        }

        public void RemoveListener(object owner, Unit unit)
        {
            listeners.RemoveAll((listener) => listener.Item1 == owner && listener.Item2 == unit);
        }
        public void RemoveListener(object owner)
        {
            listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public void Publish(Unit unit)
        {
            listeners.ForEach((listener) => {
                listener.Item3();
            });
            if (unit is Champion)
            {
                ApiEventManager.OnChampionDamageTaken.Publish((Champion)unit);
            }
        }
    }

    public class EventOnChampionDamageTaken
    {
        private List<Tuple<object, Champion, Action>> listeners = new List<Tuple<object, Champion, Action>>();
        public void AddListener(object owner, Champion champion, Action callback)
        {
            var listenerTuple = new Tuple<object, Champion, Action>(owner, champion, callback);
            listeners.Add(listenerTuple);
        }

        public void RemoveListener(object owner, Champion champion)
        {
            listeners.RemoveAll((listener) => listener.Item1 == owner && listener.Item2 == champion);
        }
        public void RemoveListener(object owner)
        {
            listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public void Publish(Champion champion)
        {
            listeners.ForEach((listener) => {
                if (listener.Item2 == champion)
                {
                    listener.Item3();
                }
            });
        }
    }
}