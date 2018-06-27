using System;
using System.Collections.Generic;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.AI;

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

        public static void RemoveAllListenersForOwner(object owner)
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
        private List<Tuple<object, Action<float>>> _listeners = new List<Tuple<object, Action<float>>>();
        public void AddListener(object owner, Action<float> callback)
        {
            var listenerTuple = new Tuple<object, Action<float>>(owner, callback);
            _listeners.Add(listenerTuple);
        }

        public void RemoveListener(object owner)
        {
            _listeners.RemoveAll(listener => listener.Item1 == owner);
        }

        public void Publish(float diff)
        {
            _listeners.ForEach(listener => listener.Item2(diff));
        }
    }

    public class EventOnUnitDamageTaken
    {
        private List<Tuple<object, AttackableUnit, Action>> _listeners = new List<Tuple<object, AttackableUnit, Action>>();
        public void AddListener(object owner, AttackableUnit unit, Action callback)
        {
            var listenerTuple = new Tuple<object, AttackableUnit, Action>(owner, unit, callback);
            _listeners.Add(listenerTuple);
        }

        public void RemoveListener(object owner, AttackableUnit unit)
        {
            _listeners.RemoveAll(listener => listener.Item1 == owner && listener.Item2 == unit);
        }

        public void RemoveListener(object owner)
        {
            _listeners.RemoveAll(listener => listener.Item1 == owner);
        }

        public void Publish(AttackableUnit unit)
        {
            _listeners.ForEach(listener => listener.Item3());
            if (unit is Champion champion)
            {
                ApiEventManager.OnChampionDamageTaken.Publish(champion);
            }
        }
    }

    public class EventOnChampionDamageTaken
    {
        private List<Tuple<object, Champion, Action>> _listeners = new List<Tuple<object, Champion, Action>>();
        public void AddListener(object owner, Champion champion, Action callback)
        {
            var listenerTuple = new Tuple<object, Champion, Action>(owner, champion, callback);
            _listeners.Add(listenerTuple);
        }

        public void RemoveListener(object owner, Champion champion)
        {
            _listeners.RemoveAll(listener => listener.Item1 == owner && listener.Item2 == champion);
        }

        public void RemoveListener(object owner)
        {
            _listeners.RemoveAll(listener => listener.Item1 == owner);
        }

        public void Publish(Champion champion)
        {
            _listeners.ForEach(listener =>
            {
                if (listener.Item2 == champion)
                {
                    listener.Item3();
                }
            });
        }
    }
}