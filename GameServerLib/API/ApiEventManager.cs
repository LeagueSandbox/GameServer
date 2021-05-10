using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Domain.GameObjects.Spell.Sector;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.Logging;
using log4net;
using System;
using System.Collections.Generic;

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
[OnSpellChannelCancel] - abrupt stop channeling
[OnSpellPostCast] - finish casting
[OnSpellPostChannel] - finish channeling
[OnSpellPreCast] - setup cast info before casting (always performed)
[OnSpellHit] - equivalent to "ApplyEffects".
[OnTakeDamage]
[OnUpdateActions] - move order probably
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
            OnCreateSector.RemoveListener(owner);
            OnHitUnit.RemoveListener(owner);
            OnLaunchAttack.RemoveListener(owner);
            OnLaunchMissile.RemoveListener(owner);
            OnPreAttack.RemoveListener(owner);
            OnSpellCast.RemoveListener(owner);
            OnSpellChannel.RemoveListener(owner);
            OnSpellChannelCancel.RemoveListener(owner);
            OnSpellMissileHit.RemoveListener(owner);
            OnSpellSectorHit.RemoveListener(owner);
            OnSpellPostCast.RemoveListener(owner);
            OnSpellPostChannel.RemoveListener(owner);
            OnTakeDamage.RemoveListener(owner);
            OnUnitCrowdControlled.RemoveListener(owner);
            OnUnitUpdateMoveOrder.RemoveListener(owner);
        }

        public static EventOnCreateSector OnCreateSector = new EventOnCreateSector();
        public static EventOnHitUnit OnHitUnit = new EventOnHitUnit();
        public static EventOnLaunchAttack OnLaunchAttack = new EventOnLaunchAttack();
        public static EventOnLaunchMissile OnLaunchMissile = new EventOnLaunchMissile();
        public static EventOnPreAttack OnPreAttack = new EventOnPreAttack();
        public static EventOnSpellCast OnSpellCast = new EventOnSpellCast();
        public static EventOnSpellChannel OnSpellChannel = new EventOnSpellChannel();
        public static EventOnSpellChannelCancel OnSpellChannelCancel = new EventOnSpellChannelCancel();
        public static EventOnSpellMissileHit OnSpellMissileHit = new EventOnSpellMissileHit();
        public static EventOnSpellSectorHit OnSpellSectorHit = new EventOnSpellSectorHit();
        public static EventOnSpellPostCast OnSpellPostCast = new EventOnSpellPostCast();
        public static EventOnSpellPostChannel OnSpellPostChannel = new EventOnSpellPostChannel();
        public static EventOnTakeDamage OnTakeDamage = new EventOnTakeDamage();
        // TODO: Handle crowd control the same as normal dashes.
        public static EventOnUnitCrowdControlled OnUnitCrowdControlled = new EventOnUnitCrowdControlled();
        // TODO: Change to OnMoveSuccess and change where Publish is called internally to reflect the name.
        public static EventOnUnitUpdateMoveOrder OnUnitUpdateMoveOrder = new EventOnUnitUpdateMoveOrder();
    }

    // TODO: Make listeners support removal at any point in code execution.

    public class EventOnCreateSector
    {
        private readonly List<Tuple<object, KeyValuePair<IObjAiBase, ISpell>, Action<ISpell, ISpellSector>, bool>> _listeners = new List<Tuple<object, KeyValuePair<IObjAiBase, ISpell>, Action<ISpell, ISpellSector>, bool>>();
        public void AddListener(object owner, KeyValuePair<IObjAiBase, ISpell> casterSpellPair, Action<ISpell, ISpellSector> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, KeyValuePair<IObjAiBase, ISpell>, Action<ISpell, ISpellSector>, bool>(owner, casterSpellPair, callback, singleInstance);
            _listeners.Add(listenerTuple);
        }
        public void RemoveListener(object owner, KeyValuePair<IObjAiBase, ISpell> casterSpellPair)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner && listener.Item2.Key == casterSpellPair.Key && listener.Item2.Value == casterSpellPair.Value);
        }
        public void RemoveListener(object owner)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public void Publish(KeyValuePair<IObjAiBase, ISpell> casterSpellPair, ISpellSector sector)
        {
            var count = _listeners.Count;

            if (count == 0)
            {
                return;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2.Key == casterSpellPair.Key && _listeners[i].Item2.Value == casterSpellPair.Value)
                {
                    _listeners[i].Item3(casterSpellPair.Value, sector);
                    if (_listeners[i].Item4 == true)
                    {
                        _listeners.RemoveAt(i);
                    }
                }
            }
        }
    }

    public class EventOnHitUnit
    {
        private readonly List<Tuple<object, IObjAiBase, Action<IAttackableUnit, bool>, bool>> _listeners = new List<Tuple<object, IObjAiBase, Action<IAttackableUnit, bool>, bool>>();
        public void AddListener(object owner, IObjAiBase unit, Action<IAttackableUnit, bool> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, IObjAiBase, Action<IAttackableUnit, bool>, bool>(owner, unit, callback, singleInstance);
            _listeners.Add(listenerTuple);
        }
        public void RemoveListener(object owner, IObjAiBase unit)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner && listener.Item2 == unit);
        }
        public void RemoveListener(object owner)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public void Publish(IObjAiBase unit, IAttackableUnit target, bool isCrit)
        {
            var count = _listeners.Count;

            if (count == 0)
            {
                return;
            }

            // TODO: Replace this method with a single function and just pass in count as a parameter (do this for all events which use this method).
            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2 == unit)
                {
                    _listeners[i].Item3(target, isCrit);
                    if (_listeners[i].Item4 == true)
                    {
                        _listeners.RemoveAt(i);
                    }
                }
            }
        }
    }

    public class EventOnLaunchAttack
    {
        private readonly List<Tuple<object, IObjAiBase, Action<ISpell>, bool>> _listeners = new List<Tuple<object, IObjAiBase, Action<ISpell>, bool>>();
        public void AddListener(object owner, IObjAiBase unit, Action<ISpell> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, IObjAiBase, Action<ISpell>, bool>(owner, unit, callback, singleInstance);
            _listeners.Add(listenerTuple);
        }
        public void RemoveListener(object owner, IObjAiBase unit)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner && listener.Item2 == unit);
        }
        public void RemoveListener(object owner)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public void Publish(IObjAiBase unit, ISpell spell)
        {
            var count = _listeners.Count;

            if (count == 0)
            {
                return;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2 == unit)
                {
                    _listeners[i].Item3(spell);
                    if (_listeners[i].Item4 == true)
                    {
                        _listeners.RemoveAt(i);
                    }
                }
            }
        }
    }

    public class EventOnLaunchMissile
    {
        private readonly List<Tuple<object, KeyValuePair<IObjAiBase, ISpell>, Action<ISpell, ISpellMissile>, bool>> _listeners = new List<Tuple<object, KeyValuePair<IObjAiBase, ISpell>, Action<ISpell, ISpellMissile>, bool>>();
        public void AddListener(object owner, KeyValuePair<IObjAiBase, ISpell> casterSpellPair, Action<ISpell, ISpellMissile> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, KeyValuePair<IObjAiBase, ISpell>, Action<ISpell, ISpellMissile>, bool>(owner, casterSpellPair, callback, singleInstance);
            _listeners.Add(listenerTuple);
        }
        public void RemoveListener(object owner, KeyValuePair<IObjAiBase, ISpell> casterSpellPair)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner && listener.Item2.Key == casterSpellPair.Key && listener.Item2.Value == casterSpellPair.Value);
        }
        public void RemoveListener(object owner)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public void Publish(KeyValuePair<IObjAiBase, ISpell> casterSpellPair, ISpellMissile missile)
        {
            var count = _listeners.Count;

            if (count == 0)
            {
                return;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2.Key == casterSpellPair.Key && _listeners[i].Item2.Value == casterSpellPair.Value)
                {
                    _listeners[i].Item3(casterSpellPair.Value, missile);
                    if (_listeners[i].Item4 == true)
                    {
                        _listeners.RemoveAt(i);
                    }
                }
            }
        }
    }

    public class EventOnPreAttack
    {
        private readonly List<Tuple<object, IObjAiBase, Action<ISpell>, bool>> _listeners = new List<Tuple<object, IObjAiBase, Action<ISpell>, bool>>();
        public void AddListener(object owner, IObjAiBase unit, Action<ISpell> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, IObjAiBase, Action<ISpell>, bool>(owner, unit, callback, singleInstance);
            _listeners.Add(listenerTuple);
        }
        public void RemoveListener(object owner, IObjAiBase unit)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner && listener.Item2 == unit);
        }
        public void RemoveListener(object owner)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public void Publish(IObjAiBase unit, ISpell spell)
        {
            var count = _listeners.Count;

            if (count == 0)
            {
                return;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2 == unit)
                {
                    _listeners[i].Item3(spell);
                    if (_listeners[i].Item4 == true)
                    {
                        _listeners.RemoveAt(i);
                    }
                }
            }
        }
    }

    public class EventOnSpellCast
    {
        private readonly List<Tuple<object, ISpell, Action<ISpell>>> _listeners = new List<Tuple<object, ISpell, Action<ISpell>>>();
        public void AddListener(object owner, ISpell spell, Action<ISpell> callback)
        {
            var listenerTuple = new Tuple<object, ISpell, Action<ISpell>>(owner, spell, callback);
            _listeners.Add(listenerTuple);
        }
        public void RemoveListener(object owner, ISpell spell)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner && listener.Item2 == spell);
        }
        public void RemoveListener(object owner)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public void Publish(ISpell spell)
        {
            var count = _listeners.Count;

            if (count == 0)
            {
                return;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2 == spell)
                {
                    _listeners[i].Item3(spell);
                }
            }
        }
    }

    public class EventOnSpellChannel
    {
        private readonly List<Tuple<object, ISpell, Action<ISpell>>> _listeners = new List<Tuple<object, ISpell, Action<ISpell>>>();
        public void AddListener(object owner, ISpell spell, Action<ISpell> callback)
        {
            var listenerTuple = new Tuple<object, ISpell, Action<ISpell>>(owner, spell, callback);
            _listeners.Add(listenerTuple);
        }
        public void RemoveListener(object owner, ISpell spell)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner && listener.Item2 == spell);
        }
        public void RemoveListener(object owner)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public void Publish(ISpell spell)
        {
            var count = _listeners.Count;

            if (count == 0)
            {
                return;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2 == spell)
                {
                    _listeners[i].Item3(spell);
                }
            }
        }
    }

    public class EventOnSpellChannelCancel
    {
        private readonly List<Tuple<object, ISpell, Action<ISpell>>> _listeners = new List<Tuple<object, ISpell, Action<ISpell>>>();
        public void AddListener(object owner, ISpell spell, Action<ISpell> callback)
        {
            var listenerTuple = new Tuple<object, ISpell, Action<ISpell>>(owner, spell, callback);
            _listeners.Add(listenerTuple);
        }
        public void RemoveListener(object owner, ISpell spell)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner && listener.Item2 == spell);
        }
        public void RemoveListener(object owner)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public void Publish(ISpell spell)
        {
            var count = _listeners.Count;

            if (count == 0)
            {
                return;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2 == spell)
                {
                    _listeners[i].Item3(spell);
                }
            }
        }
    }

    public class EventOnSpellMissileHit
    {
        private readonly List<Tuple<object, KeyValuePair<ISpell, IObjAiBase>, Action<ISpell, IAttackableUnit, ISpellMissile>, bool>> _listeners = new List<Tuple<object, KeyValuePair<ISpell, IObjAiBase>, Action<ISpell, IAttackableUnit, ISpellMissile>, bool>>();
        public void AddListener(object owner, KeyValuePair<ISpell, IObjAiBase> spellWithOwner, Action<ISpell, IAttackableUnit, ISpellMissile> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, KeyValuePair<ISpell, IObjAiBase>, Action<ISpell, IAttackableUnit, ISpellMissile>, bool>(owner, spellWithOwner, callback, singleInstance);
            _listeners.Add(listenerTuple);
        }
        public void RemoveListener(object owner, KeyValuePair<ISpell, IObjAiBase> spellWithOwner)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner && listener.Item2.Key == spellWithOwner.Key && listener.Item2.Value == spellWithOwner.Value);
        }
        public void RemoveListener(object owner)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public void Publish(IObjAiBase unit, ISpell spell, IAttackableUnit target, ISpellMissile p)
        {
            var count = _listeners.Count;

            if (count == 0)
            {
                return;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2.Key == spell && _listeners[i].Item2.Value == unit)
                {
                    _listeners[i].Item3(spell, target, p);
                    if (_listeners[i].Item4 == true)
                    {
                        _listeners.RemoveAt(i);
                    }
                }
            }
        }
    }

    public class EventOnSpellSectorHit
    {
        private readonly List<Tuple<object, KeyValuePair<ISpell, IObjAiBase>, Action<ISpell, IAttackableUnit, ISpellSector>, bool>> _listeners = new List<Tuple<object, KeyValuePair<ISpell, IObjAiBase>, Action<ISpell, IAttackableUnit, ISpellSector>, bool>>();
        public void AddListener(object owner, KeyValuePair<ISpell, IObjAiBase> spellWithOwner, Action<ISpell, IAttackableUnit, ISpellSector> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, KeyValuePair<ISpell, IObjAiBase>, Action<ISpell, IAttackableUnit, ISpellSector>, bool>(owner, spellWithOwner, callback, singleInstance);
            _listeners.Add(listenerTuple);
        }
        public void RemoveListener(object owner, KeyValuePair<ISpell, IObjAiBase> spellWithOwner)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner && listener.Item2.Key == spellWithOwner.Key && listener.Item2.Value == spellWithOwner.Value);
        }
        public void RemoveListener(object owner)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public void Publish(IObjAiBase unit, ISpell spell, IAttackableUnit target, ISpellSector s)
        {
            var count = _listeners.Count;

            if (count == 0)
            {
                return;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2.Key == spell && _listeners[i].Item2.Value == unit)
                {
                    _listeners[i].Item3(spell, target, s);
                    if (_listeners[i].Item4 == true)
                    {
                        _listeners.RemoveAt(i);
                    }
                }
            }
        }
    }

    public class EventOnSpellPostCast
    {
        private readonly List<Tuple<object, ISpell, Action<ISpell>>> _listeners = new List<Tuple<object, ISpell, Action<ISpell>>>();
        public void AddListener(object owner, ISpell spell, Action<ISpell> callback)
        {
            var listenerTuple = new Tuple<object, ISpell, Action<ISpell>>(owner, spell, callback);
            _listeners.Add(listenerTuple);
        }
        public void RemoveListener(object owner, ISpell spell)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner && listener.Item2 == spell);
        }
        public void RemoveListener(object owner)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public void Publish(ISpell spell)
        {
            var count = _listeners.Count;

            if (count == 0)
            {
                return;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2 == spell)
                {
                    _listeners[i].Item3(spell);
                }
            }
        }
    }

    public class EventOnSpellPostChannel
    {
        private readonly List<Tuple<object, ISpell, Action<ISpell>>> _listeners = new List<Tuple<object, ISpell, Action<ISpell>>>();
        public void AddListener(object owner, ISpell spell, Action<ISpell> callback)
        {
            var listenerTuple = new Tuple<object, ISpell, Action<ISpell>>(owner, spell, callback);
            _listeners.Add(listenerTuple);
        }
        public void RemoveListener(object owner, ISpell spell)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner && listener.Item2 == spell);
        }
        public void RemoveListener(object owner)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public void Publish(ISpell spell)
        {
            var count = _listeners.Count;

            if (count == 0)
            {
                return;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2 == spell)
                {
                    _listeners[i].Item3(spell);
                }
            }
        }
    }

    public class EventOnTakeDamage
    {
        private readonly List<Tuple<object, IAttackableUnit, Action<IAttackableUnit, IAttackableUnit>, bool>> _listeners = new List<Tuple<object, IAttackableUnit, Action<IAttackableUnit, IAttackableUnit>, bool>>();
        /// <summary>
        /// Adds a listener for this event, wherein, if the unit that took damage was the given unit, it will call the <paramref name="callback"/> function.
        /// </summary>
        /// <param name="owner">Object which will own this listener. Used in removal. Often times "this" will suffice.</param>
        /// <param name="unit">Unit that should be checked when this event fires.</param>
        /// <param name="callback">Function to call when this event fires.</param>
        /// <param name="singleInstance">Whether or not to remove the event listener after calling the <paramref name="callback"/> function.</param>
        public void AddListener(object owner, IAttackableUnit unit, Action<IAttackableUnit, IAttackableUnit> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, IAttackableUnit, Action<IAttackableUnit, IAttackableUnit>, bool>(owner, unit, callback, singleInstance);
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

        public void Publish(IAttackableUnit unit, IAttackableUnit source)
        {
            var count = _listeners.Count;

            if (count == 0)
            {
                return;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2 == unit)
                {
                    _listeners[i].Item3(unit, source);
                    if (_listeners[i].Item4 == true)
                    {
                        _listeners.RemoveAt(i);
                    }
                }
            }
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
        private readonly List<Tuple<object, IObjAiBase, Action<IObjAiBase, OrderType>, bool>> _listeners = new List<Tuple<object, IObjAiBase, Action<IObjAiBase, OrderType>, bool>>();
        public void AddListener(object owner, IObjAiBase unit, Action<IObjAiBase, OrderType> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, IObjAiBase, Action<IObjAiBase, OrderType>, bool>(owner, unit, callback, singleInstance);
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

        public void Publish(IObjAiBase unit, OrderType order)
        {
            var count = _listeners.Count;

            if (count == 0)
            {
                return;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2 == unit)
                {
                    _listeners[i].Item3(unit, order);
                    if (_listeners[i].Item4 == true)
                    {
                        _listeners.RemoveAt(i);
                    }
                }
            }
        }
    }
}