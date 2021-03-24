using System;
using System.Collections.Generic;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Logging;
using log4net;

/*
 * Possible Events:
 * Events that are always performed are accounted for in-script, no event handling needed. EX: Spell calls spellscript.OnActivate()
[OnActivate] - buffs and spells (always performed)
[OnAddPAR]
[OnAllowAdd]
[OnAssist]
[OnAssistUnit]
[OnBeingDodged]
[OnBeingHit]
[OnBeingSpellHit]
[OnCollision]
[OnCollisionTerrain]
[OnDeactivate] - buffs and spells (always performed)
[OnDealDamage]
[OnDeath]
[OnDisconnect]
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
[OnPreTakeDamage]
[OnReconnect]
[OnResurrect]
[OnSpellCast] - start casting
[OnSpellChannel] - start channeling
[OnSpellPostCast] - finish casting
[OnSpellPostChannel] - finish channeling
[OnSpellPreCast] - setup cast info before casting (always performed)
[OnSpellHit] - equivalent to "ApplyEffects".
[OnTakeDamage]
[OnUpdateActions] - buffs and spells (always performed)
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
            OnHitUnit.RemoveListener(owner);
            OnLaunchAttack.RemoveListener(owner);
            OnPreAttack.RemoveListener(owner);
            OnSpellCast.RemoveListener(owner);
            OnSpellChannel.RemoveListener(owner);
            OnSpellHit.RemoveListener(owner);
            OnSpellPostCast.RemoveListener(owner);
            OnSpellPostChannel.RemoveListener(owner);
            OnTakeDamage.RemoveListener(owner);
            OnUnitCrowdControlled.RemoveListener(owner);
            OnUnitUpdateMoveOrder.RemoveListener(owner);
        }

        public static EventOnHitUnit OnHitUnit = new EventOnHitUnit();
        public static EventOnLaunchAttack OnLaunchAttack = new EventOnLaunchAttack();
        public static EventOnPreAttack OnPreAttack = new EventOnPreAttack();
        public static EventOnSpellCast OnSpellCast = new EventOnSpellCast();
        public static EventOnSpellChannel OnSpellChannel = new EventOnSpellChannel();
        public static EventOnSpellHit OnSpellHit = new EventOnSpellHit();
        public static EventOnSpellPostCast OnSpellPostCast = new EventOnSpellPostCast();
        public static EventOnSpellPostChannel OnSpellPostChannel = new EventOnSpellPostChannel();
        public static EventOnTakeDamage OnTakeDamage = new EventOnTakeDamage();
        // TODO: Handle crowd control the same as normal dashes.
        public static EventOnUnitCrowdControlled OnUnitCrowdControlled = new EventOnUnitCrowdControlled();
        // TODO: Change to OnMoveSuccess and change where Publish is called internally to reflect the name.
        public static EventOnUnitUpdateMoveOrder OnUnitUpdateMoveOrder = new EventOnUnitUpdateMoveOrder();
    }

    public class EventOnHitUnit
    {
        private List<Tuple<object, IObjAiBase, Action<IAttackableUnit, bool>, bool>> listeners = new List<Tuple<object, IObjAiBase, Action<IAttackableUnit, bool>, bool>>();
        public void AddListener(object owner, IObjAiBase unit, Action<IAttackableUnit, bool> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, IObjAiBase, Action<IAttackableUnit, bool>, bool>(owner, unit, callback, singleInstance);
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
            var count = listeners.Count;

            if (count == 0)
            {
                return;
            }

            for (int i = 0; i < count && count > 0; i++)
            {
                if (listeners[i].Item2 == unit)
                {
                    listeners[i].Item3(target, isCrit);
                    if (listeners[i].Item4 == true)
                    {
                        listeners.RemoveAt(i);
                        i--;
                        count--;
                    }
                }
            }
        }
    }

    public class EventOnLaunchAttack
    {
        private List<Tuple<object, IObjAiBase, Action<ISpell>, bool>> listeners = new List<Tuple<object, IObjAiBase, Action<ISpell>, bool>>();
        public void AddListener(object owner, IObjAiBase unit, Action<ISpell> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, IObjAiBase, Action<ISpell>, bool>(owner, unit, callback, singleInstance);
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
        public void Publish(IObjAiBase unit, ISpell spell)
        {
            var count = listeners.Count;

            if (count == 0)
            {
                return;
            }

            for (int i = 0; i < count && count > 0; i++)
            {
                if (listeners[i].Item2 == unit)
                {
                    listeners[i].Item3(spell);
                    if (listeners[i].Item4 == true)
                    {
                        listeners.RemoveAt(i);
                        i--;
                        count--;
                    }
                }
            }
        }
    }

    public class EventOnPreAttack
    {
        private List<Tuple<object, IObjAiBase, Action<ISpell>, bool>> listeners = new List<Tuple<object, IObjAiBase, Action<ISpell>, bool>>();
        public void AddListener(object owner, IObjAiBase unit, Action<ISpell> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, IObjAiBase, Action<ISpell>, bool>(owner, unit, callback, singleInstance);
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
        public void Publish(IObjAiBase unit, ISpell spell)
        {
            var count = listeners.Count;

            if (count == 0)
            {
                return;
            }

            for (int i = 0; i < count && count > 0; i++)
            {
                if (listeners[i].Item2 == unit)
                {
                    listeners[i].Item3(spell);
                    if (listeners[i].Item4 == true)
                    {
                        listeners.RemoveAt(i);
                        i--;
                        count--;
                    }
                }
            }
        }
    }

    public class EventOnSpellCast
    {
        private List<Tuple<object, ISpell, Action<ISpell>>> listeners = new List<Tuple<object, ISpell, Action<ISpell>>>();
        public void AddListener(object owner, ISpell spell, Action<ISpell> callback)
        {
            var listenerTuple = new Tuple<object, ISpell, Action<ISpell>>(owner, spell, callback);
            listeners.Add(listenerTuple);
        }
        public void RemoveListener(object owner, ISpell spell)
        {
            listeners.RemoveAll((listener) => listener.Item1 == owner && listener.Item2 == spell);
        }
        public void RemoveListener(object owner)
        {
            listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public void Publish(ISpell spell)
        {
            listeners.ForEach((listener) =>
            {
                if (listener.Item2 == spell)
                {
                    listener.Item3(spell);
                }
            });
        }
    }

    public class EventOnSpellChannel
    {
        private List<Tuple<object, ISpell, Action<ISpell>>> listeners = new List<Tuple<object, ISpell, Action<ISpell>>>();
        public void AddListener(object owner, ISpell spell, Action<ISpell> callback)
        {
            var listenerTuple = new Tuple<object, ISpell, Action<ISpell>>(owner, spell, callback);
            listeners.Add(listenerTuple);
        }
        public void RemoveListener(object owner, ISpell spell)
        {
            listeners.RemoveAll((listener) => listener.Item1 == owner && listener.Item2 == spell);
        }
        public void RemoveListener(object owner)
        {
            listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public void Publish(ISpell spell)
        {
            listeners.ForEach((listener) =>
            {
                if (listener.Item2 == spell)
                {
                    listener.Item3(spell);
                }
            });
        }
    }

    public class EventOnSpellHit
    {
        private List<Tuple<object, KeyValuePair<ISpell, IObjAiBase>, Action<ISpell, IAttackableUnit, IProjectile>, bool>> listeners = new List<Tuple<object, KeyValuePair<ISpell, IObjAiBase>, Action<ISpell, IAttackableUnit, IProjectile>, bool>>();
        public void AddListener(object owner, KeyValuePair<ISpell, IObjAiBase> spellWithOwner, Action<ISpell, IAttackableUnit, IProjectile> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, KeyValuePair<ISpell, IObjAiBase>, Action<ISpell, IAttackableUnit, IProjectile>, bool>(owner, spellWithOwner, callback, singleInstance);
            listeners.Add(listenerTuple);
        }
        public void RemoveListener(object owner, KeyValuePair<ISpell, IObjAiBase> spellWithOwner)
        {
            listeners.RemoveAll((listener) => listener.Item1 == owner && listener.Item2.Key == spellWithOwner.Key && listener.Item2.Value == spellWithOwner.Value);
        }
        public void RemoveListener(object owner)
        {
            listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public void Publish(IObjAiBase unit, ISpell spell, IAttackableUnit target, IProjectile p)
        {
            var count = listeners.Count;

            if (count == 0)
            {
                return;
            }

            for (int i = 0; i < count && count > 0; i++)
            {
                if (listeners[i].Item2.Key == spell && listeners[i].Item2.Value == unit)
                {
                    listeners[i].Item3(spell, target, p);
                    if (listeners[i].Item4 == true)
                    {
                        listeners.RemoveAt(i);
                        i--;
                        count--;
                    }
                }
            }
        }
    }

    public class EventOnSpellPostCast
    {
        private List<Tuple<object, ISpell, Action<ISpell>>> listeners = new List<Tuple<object, ISpell, Action<ISpell>>>();
        public void AddListener(object owner, ISpell spell, Action<ISpell> callback)
        {
            var listenerTuple = new Tuple<object, ISpell, Action<ISpell>>(owner, spell, callback);
            listeners.Add(listenerTuple);
        }
        public void RemoveListener(object owner, ISpell spell)
        {
            listeners.RemoveAll((listener) => listener.Item1 == owner && listener.Item2 == spell);
        }
        public void RemoveListener(object owner)
        {
            listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public void Publish(ISpell spell)
        {
            listeners.ForEach((listener) =>
            {
                if (listener.Item2 == spell)
                {
                    listener.Item3(spell);
                }
            });
        }
    }

    public class EventOnSpellPostChannel
    {
        private List<Tuple<object, ISpell, Action<ISpell>>> listeners = new List<Tuple<object, ISpell, Action<ISpell>>>();
        public void AddListener(object owner, ISpell spell, Action<ISpell> callback)
        {
            var listenerTuple = new Tuple<object, ISpell, Action<ISpell>>(owner, spell, callback);
            listeners.Add(listenerTuple);
        }
        public void RemoveListener(object owner, ISpell spell)
        {
            listeners.RemoveAll((listener) => listener.Item1 == owner && listener.Item2 == spell);
        }
        public void RemoveListener(object owner)
        {
            listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public void Publish(ISpell spell)
        {
            listeners.ForEach((listener) =>
            {
                if (listener.Item2 == spell)
                {
                    listener.Item3(spell);
                }
            });
        }
    }

    public class EventOnTakeDamage
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
        }
    }

    public class EventOnUnitCrowdControlled
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
        }
    }

    public class EventOnUnitUpdateMoveOrder
    {
        private readonly List<Tuple<object, IObjAiBase, Action>> _listeners = new List<Tuple<object, IObjAiBase, Action>>();
        public void AddListener(object owner, IObjAiBase unit, Action callback)
        {
            var listenerTuple = new Tuple<object, IObjAiBase, Action>(owner, unit, callback);
            _listeners.Add(listenerTuple);
        }

        public void RemoveListener(object owner, IObjAiBase unit)
        {
            _listeners.RemoveAll(listener => listener.Item1 == owner && listener.Item2 == unit);
        }

        public void RemoveListener(object owner)
        {
            _listeners.RemoveAll(listener => listener.Item1 == owner);
        }

        public void Publish(IObjAiBase unit)
        {
            _listeners.ForEach(listener =>
            {
                if (listener.Item2 == unit)
                {
                    listener.Item3();
                }
            });
        }
    }
}