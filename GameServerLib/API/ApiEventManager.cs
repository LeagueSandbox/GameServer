using System;
using System.Collections.Generic;
using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Logging;
using log4net;

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

namespace LeagueSandbox.GameServer.API
{
    public static class ApiEventManager
    {
        private static Game _game;
        private static ILog _logger;

        internal static void SetGame(Game game)
        {
            _game = game;
            _logger = LoggerProvider.GetLogger();
        }

        public static void RemoveAllListenersForOwner(object owner)
        {
            OnChampionDamageTaken.RemoveListener(owner);
            OnUpdate.RemoveListener(owner);
        }

        public static EventOnUpdate OnUpdate = new EventOnUpdate();
        public static EventOnChampionDamageTaken OnChampionDamageTaken = new EventOnChampionDamageTaken();
        public static EventOnUnitDamageTaken OnUnitDamageTaken = new EventOnUnitDamageTaken();
        public static EventOnHitUnit OnHitUnit = new EventOnHitUnit();
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
        private readonly List<Tuple<object, IAttackableUnit, Action>> _listeners = new List<Tuple<object, IAttackableUnit, Action>>();
        public void AddListener(object owner, IAttackableUnit unit, Action callback)
        {
            var listenerTuple = new Tuple<object, IAttackableUnit, Action>(owner, unit, callback);
            _listeners.Add(listenerTuple);
        }

        public void RemoveListener(object owner, IAttackableUnit unit)
        {
            _listeners.RemoveAll(listener => listener.Item1 == owner && listener.Item2 == unit);
        }

        public void RemoveListener(object owner)
        {
            _listeners.RemoveAll(listener => listener.Item1 == owner);
        }

        public void Publish(IAttackableUnit unit)
        {
            _listeners.ForEach(listener => listener.Item3());
            if (unit is IChampion champion)
                ApiEventManager.OnChampionDamageTaken.Publish(champion);
        }
    }

    public class EventOnChampionDamageTaken
    {
        private readonly List<Tuple<object, IChampion, Action>> _listeners = new List<Tuple<object, IChampion, Action>>();
        public void AddListener(object owner, IChampion champion, Action callback)
        {
            var listenerTuple = new Tuple<object, IChampion, Action>(owner, champion, callback);
            _listeners.Add(listenerTuple);
        }

        public void RemoveListener(object owner, IChampion champion)
        {
            _listeners.RemoveAll(listener => listener.Item1 == owner && listener.Item2 == champion);
        }

        public void RemoveListener(object owner)
        {
            _listeners.RemoveAll(listener => listener.Item1 == owner);
        }

        public void Publish(IChampion champion)
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

    public class EventOnHitUnit
    {
        private List<Tuple<object, IObjAiBase, Action<IAttackableUnit, bool>>> listeners = new List<Tuple<object, IObjAiBase, Action<IAttackableUnit, bool>>>();
        public void AddListener(object owner, IObjAiBase unit, Action<IAttackableUnit, bool> callback)
        {
            var listenerTuple = new Tuple<object, IObjAiBase, Action<IAttackableUnit, bool>>(owner, unit, callback);
            listeners.Add(listenerTuple);
        }
        public void RemoveListener(object owner, IObjAiBase unit)
        {
            listeners.RemoveAll((listener) => listener.Item1 == owner && listener.Item2 == unit);
        }
        public void RemoveListener(object owner)
        {
            listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public void Publish(IObjAiBase unit, IAttackableUnit target, bool isCrit)
        {
            listeners.ForEach((listener) =>
            {
                if (listener.Item2 == unit)
                {
                    listener.Item3(target, isCrit);
                }
            });
        }
        public void AddListener(IChampion owner, Action<IAttackableUnit, bool> onAutoAttack)
        {
            throw new NotImplementedException();
        }
    }
}