using GameServerCore.Domain;
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
[OnAllowAddBuff]
[OnAssist]
[OnAssistUnit]
[OnBeingDodged]
[OnBeingHit]
[OnBeingSpellHit]
[OnCanCast]
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
[OnSpellHit] - "ApplyEffects" function in Spell.
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
            OnAddPAR.RemoveListener(owner);
            OnAllowAddBuff.RemoveListener(owner);
            OnBeingHit.RemoveListener(owner);
            OnBeingSpellHit.RemoveListener(owner);
            OnBuffDeactivated.RemoveListener(owner);
            OnCanCast.RemoveListener(owner);
            OnCollision.RemoveListener(owner);
            OnCollisionTerrain.RemoveListener(owner);
            OnCreateSector.RemoveListener(owner);
            OnDealDamage.RemoveListener(owner);
            OnDeath.RemoveListener(owner);
            OnHitUnit.RemoveListener(owner);
            OnIncrementChampionScore.RemoveListener(owner);
            OnKill.RemoveListener(owner);
            OnKillUnit.RemoveListener(owner);
            OnLaunchAttack.RemoveListener(owner);
            OnLaunchMissile.RemoveListener(owner);
            OnLevelUp.RemoveListener(owner);
            OnLevelUpSpell.RemoveListener(owner);
            OnMoveEnd.RemoveListener(owner);
            OnMoveFailure.RemoveListener(owner);
            OnMoveSuccess.RemoveListener(owner);
            OnPreAttack.RemoveListener(owner);
            OnPreDealDamage.RemoveListener(owner);
            OnPreTakeDamage.RemoveListener(owner);
            OnResurrect.RemoveListener(owner);
            OnSpellCast.RemoveListener(owner);
            OnSpellChannel.RemoveListener(owner);
            OnSpellChannelCancel.RemoveListener(owner);
            OnSpellHit.RemoveListener(owner);
            OnSpellMissileEnd.RemoveListener(owner);
            OnSpellMissileHit.RemoveListener(owner);
            OnSpellMissileUpdate.RemoveListener(owner);
            OnSpellPostCast.RemoveListener(owner);
            OnSpellPostChannel.RemoveListener(owner);
            OnSpellSectorHit.RemoveListener(owner);
            OnTakeDamage.RemoveListener(owner);
            OnTargetLost.RemoveListener(owner);
            OnUnitBuffDeactivated.RemoveListener(owner);
            OnUnitCrowdControlled.RemoveListener(owner);
            OnUnitUpdateMoveOrder.RemoveListener(owner);
            OnUpdateStats.RemoveListener(owner);
        }

        // Unused
        public static EventOnAddPAR OnAddPAR = new EventOnAddPAR();
        public static EventOnAllowAddBuff OnAllowAddBuff = new EventOnAllowAddBuff();
        public static EventOnBeingHit OnBeingHit = new EventOnBeingHit();
        public static EventOnBeingSpellHit OnBeingSpellHit = new EventOnBeingSpellHit();
        public static EventOnBuffDeactivated OnBuffDeactivated = new EventOnBuffDeactivated();
        public static EventOnCanCast OnCanCast = new EventOnCanCast();
        public static EventOnCollision OnCollision = new EventOnCollision();
        public static EventOnCollisionTerrain OnCollisionTerrain = new EventOnCollisionTerrain();
        public static EventOnCreateSector OnCreateSector = new EventOnCreateSector();
        public static EventOnDealDamage OnDealDamage = new EventOnDealDamage();
        public static EventOnDeath OnDeath = new EventOnDeath();
        public static EventOnHitUnit OnHitUnit = new EventOnHitUnit();
        public static EventOnIncrementChampionScore OnIncrementChampionScore = new EventOnIncrementChampionScore();
        public static EventOnKill OnKill = new EventOnKill();
        public static EventOnKillUnit OnKillUnit = new EventOnKillUnit();
        public static EventOnLaunchAttack OnLaunchAttack = new EventOnLaunchAttack();
        /// <summary>
        /// Called immediately after the rocket is added to the scene. *NOTE*: At the time of the call, the rocket has not yet been spawned for players.
        /// <summary>
        public static EventOnLaunchMissile OnLaunchMissile = new EventOnLaunchMissile();
        public static EventOnLevelUp OnLevelUp = new EventOnLevelUp();
        public static EventOnLevelUpSpell OnLevelUpSpell = new EventOnLevelUpSpell();
        public static EventOnMoveEnd OnMoveEnd = new EventOnMoveEnd();
        public static EventOnMoveFailure OnMoveFailure = new EventOnMoveFailure();
        public static EventOnMoveSuccess OnMoveSuccess = new EventOnMoveSuccess();
        public static EventOnPreAttack OnPreAttack = new EventOnPreAttack();
        public static EventOnPreDealDamage OnPreDealDamage = new EventOnPreDealDamage();
        public static EventOnPreTakeDamage OnPreTakeDamage = new EventOnPreTakeDamage();
        public static EventOnResurrect OnResurrect = new EventOnResurrect();
        public static EventOnSpellCast OnSpellCast = new EventOnSpellCast();
        public static EventOnSpellChannel OnSpellChannel = new EventOnSpellChannel();
        public static EventOnSpellChannelCancel OnSpellChannelCancel = new EventOnSpellChannelCancel();
        public static EventOnSpellHit OnSpellHit = new EventOnSpellHit();
        public static EventOnSpellMissileEnd OnSpellMissileEnd = new EventOnSpellMissileEnd();
        public static EventOnSpellMissileHit OnSpellMissileHit = new EventOnSpellMissileHit();
        public static EventOnSpellMissileUpdate OnSpellMissileUpdate = new EventOnSpellMissileUpdate();
        public static EventOnSpellPostCast OnSpellPostCast = new EventOnSpellPostCast();
        public static EventOnSpellPostChannel OnSpellPostChannel = new EventOnSpellPostChannel();
        public static EventOnSpellSectorHit OnSpellSectorHit = new EventOnSpellSectorHit();
        public static EventOnTakeDamage OnTakeDamage = new EventOnTakeDamage();
        public static EventOnTargetLost OnTargetLost = new EventOnTargetLost();
        public static EventOnUnitBuffDeactivated OnUnitBuffDeactivated = new EventOnUnitBuffDeactivated();
        // TODO: Handle crowd control the same as normal dashes.
        public static EventOnUnitCrowdControlled OnUnitCrowdControlled = new EventOnUnitCrowdControlled();
        // TODO: Change to OnMoveSuccess and change where Publish is called internally to reflect the name.
        public static EventOnUnitUpdateMoveOrder OnUnitUpdateMoveOrder = new EventOnUnitUpdateMoveOrder();
        public static EventOnUpdateStats OnUpdateStats = new EventOnUpdateStats();
    }

    // TODO: Make listeners support removal at any point in code execution.

    public class EventOnAddPAR
    {
        private readonly List<Tuple<object, IAttackableUnit, Action<IAttackableUnit, IAttackableUnit>, bool>> _listeners = new List<Tuple<object, IAttackableUnit, Action<IAttackableUnit, IAttackableUnit>, bool>>();
        public void AddListener(object owner, IAttackableUnit unit, Action<IAttackableUnit, IAttackableUnit> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, IAttackableUnit, Action<IAttackableUnit, IAttackableUnit>, bool>(owner, unit, callback, singleInstance);
            _listeners.Add(listenerTuple);
        }
        public void RemoveListener(object owner, IAttackableUnit unit)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner && listener.Item2 == unit);
        }
        public void RemoveListener(object owner)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public void Publish(IAttackableUnit target, IAttackableUnit source)
        {
            var count = _listeners.Count;

            if (count == 0)
            {
                return;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2 == target)
                {
                    var listener = _listeners[i];
                    listener.Item3(target, source);

                    if (listener.Item4)
                    {
                        _listeners.RemoveAt(i);
                    }
                }
            }
        }
    }

    public class EventOnAllowAddBuff
    {
        private readonly List<Tuple<object, IAttackableUnit, Func<IAttackableUnit, IAttackableUnit, IBuff, bool>, bool>> _listeners = new List<Tuple<object, IAttackableUnit, Func<IAttackableUnit, IAttackableUnit, IBuff, bool>, bool>>();
        public void AddListener(object owner, IAttackableUnit unit, Func<IAttackableUnit, IAttackableUnit, IBuff, bool> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, IAttackableUnit, Func<IAttackableUnit, IAttackableUnit, IBuff, bool>, bool>(owner, unit, callback, singleInstance);
            _listeners.Add(listenerTuple);
        }
        public void RemoveListener(object owner, IAttackableUnit unit)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner && listener.Item2 == unit);
        }
        public void RemoveListener(object owner)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public bool Publish(IAttackableUnit target, IAttackableUnit source, IBuff buff)
        {
            bool returnVal = true;

            var count = _listeners.Count;

            if (count == 0)
            {
                return returnVal;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2 == target)
                {
                    var listener = _listeners[i];
                    returnVal = listener.Item3(target, source, buff);

                    if (listener.Item4)
                    {
                        _listeners.RemoveAt(i);
                    }
                }
            }

            return returnVal;
        }
    }

    public class EventOnBeingHit
    {
        private readonly List<Tuple<object, IAttackableUnit, Action<IAttackableUnit, IAttackableUnit>, bool>> _listeners = new List<Tuple<object, IAttackableUnit, Action<IAttackableUnit, IAttackableUnit>, bool>>();
        public void AddListener(object owner, IAttackableUnit unit, Action<IAttackableUnit, IAttackableUnit> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, IAttackableUnit, Action<IAttackableUnit, IAttackableUnit>, bool>(owner, unit, callback, singleInstance);
            _listeners.Add(listenerTuple);
        }
        public void RemoveListener(object owner, IAttackableUnit unit)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner && listener.Item2 == unit);
        }
        public void RemoveListener(object owner)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public void Publish(IAttackableUnit target, IAttackableUnit attacker)
        {
            var count = _listeners.Count;

            if (count == 0)
            {
                return;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2 == target)
                {
                    var listener = _listeners[i];
                    listener.Item3(target, attacker);

                    if (listener.Item4)
                    {
                        _listeners.Remove(listener);
                    }
                }
            }
        }
    }

    public class EventOnBeingSpellHit
    {
        private readonly List<Tuple<object, IAttackableUnit, Action<IAttackableUnit, IAttackableUnit, ISpell, ISpellMissile, ISpellSector>, bool>> _listeners = new List<Tuple<object, IAttackableUnit, Action<IAttackableUnit, IAttackableUnit, ISpell, ISpellMissile, ISpellSector>, bool>>();
        public void AddListener(object owner, IAttackableUnit unit, Action<IAttackableUnit, IAttackableUnit, ISpell, ISpellMissile, ISpellSector> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, IAttackableUnit, Action<IAttackableUnit, IAttackableUnit, ISpell, ISpellMissile, ISpellSector>, bool>(owner, unit, callback, singleInstance);
            _listeners.Add(listenerTuple);
        }
        public void RemoveListener(object owner, IAttackableUnit unit)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner && listener.Item2 == unit);
        }
        public void RemoveListener(object owner)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public void Publish(IAttackableUnit target, IAttackableUnit attacker, ISpell spell, ISpellMissile missile, ISpellSector sector)
        {
            var count = _listeners.Count;

            if (count == 0)
            {
                return;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2 == target)
                {
                    var listener = _listeners[i];
                    listener.Item3(target, attacker, spell, missile, sector);

                    if (listener.Item4)
                    {
                        _listeners.Remove(listener);
                    }
                }
            }
        }
    }

    public class EventOnBuffDeactivated
    {
        private readonly List<Tuple<object, IBuff, Action<IBuff>, bool>> _listeners = new List<Tuple<object, IBuff, Action<IBuff>, bool>>();
        public void AddListener(object owner, IBuff buff, Action<IBuff> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, IBuff, Action<IBuff>, bool>(owner, buff, callback, singleInstance);
            _listeners.Add(listenerTuple);
        }
        public void RemoveListener(object owner)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public void Publish(IBuff buff)
        {
            var count = _listeners.Count;

            if (count == 0)
            {
                return;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2 == buff)
                {
                    var listener = _listeners[i];
                    listener.Item3(buff);

                    if (listener.Item4)
                    {
                        _listeners.Remove(listener);
                    }
                }
            }
        }
    }

    public class EventOnCanCast
    {
        private readonly List<Tuple<object, IAttackableUnit, Func<IAttackableUnit, ISpell, bool>, bool>> _listeners = new List<Tuple<object, IAttackableUnit, Func<IAttackableUnit, ISpell, bool>, bool>>();
        public void AddListener(object owner, IAttackableUnit unit, Func<IAttackableUnit, ISpell, bool> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, IAttackableUnit, Func<IAttackableUnit, ISpell, bool>, bool>(owner, unit, callback, singleInstance);
            _listeners.Add(listenerTuple);
        }
        public void RemoveListener(object owner, IAttackableUnit unit)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner && listener.Item2 == unit);
        }
        public void RemoveListener(object owner)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public bool Publish(IAttackableUnit target, ISpell spell)
        {
            bool returnVal = true;

            var count = _listeners.Count;

            if (count == 0)
            {
                return returnVal;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2 == target)
                {
                    var listener = _listeners[i];
                    returnVal = listener.Item3(target, spell);

                    if (listener.Item4)
                    {
                        _listeners.RemoveAt(i);
                    }
                }
            }

            return returnVal;
        }
    }

    public class EventOnCollision
    {
        private readonly List<Tuple<object, IGameObject, Action<IGameObject, IGameObject>, bool>> _listeners = new List<Tuple<object, IGameObject, Action<IGameObject, IGameObject>, bool>>();
        public void AddListener(object owner, IGameObject obj, Action<IGameObject, IGameObject> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, IGameObject, Action<IGameObject, IGameObject>, bool>(owner, obj, callback, singleInstance);
            _listeners.Add(listenerTuple);
        }
        public void RemoveListener(object owner, IGameObject obj)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner && listener.Item2 == obj);
        }
        public void RemoveListener(object owner)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public void Publish(IGameObject owner, IGameObject target)
        {
            var count = _listeners.Count;

            if (count == 0)
            {
                return;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2 == owner)
                {
                    var listener = _listeners[i];
                    listener.Item3(owner, target);

                    if (listener.Item4)
                    {
                        _listeners.Remove(listener);
                    }
                }
            }
        }
    }

    public class EventOnCollisionTerrain
    {
        private readonly List<Tuple<object, IGameObject, Action<IGameObject>, bool>> _listeners = new List<Tuple<object, IGameObject, Action<IGameObject>, bool>>();
        public void AddListener(object owner, IGameObject obj, Action<IGameObject> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, IGameObject, Action<IGameObject>, bool>(owner, obj, callback, singleInstance);
            _listeners.Add(listenerTuple);
        }
        public void RemoveListener(object owner, IGameObject obj)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner && listener.Item2 == obj);
        }
        public void RemoveListener(object owner)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public void Publish(IGameObject owner)
        {
            var count = _listeners.Count;

            if (count == 0)
            {
                return;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2 == owner)
                {
                    var listener = _listeners[i];
                    listener.Item3(owner);

                    if (listener.Item4)
                    {
                        _listeners.Remove(listener);
                    }
                }
            }
        }
    }

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
                    var listener = _listeners[i];
                    listener.Item3(casterSpellPair.Value, sector);

                    if (listener.Item4)
                    {
                        _listeners.Remove(listener);
                    }
                }
            }
        }
    }

    public class EventOnDealDamage
    {
        private readonly List<Tuple<object, IAttackableUnit, Action<IDamageData>, bool>> _listeners = new List<Tuple<object, IAttackableUnit, Action<IDamageData>, bool>>();
        /// <summary>
        /// Adds a listener for this event, wherein, if the unit that took damage was the given unit, it will call the <paramref name="callback"/> function.
        /// </summary>
        /// <param name="owner">Object which will own this listener. Used in removal. Often times "this" will suffice.</param>
        /// <param name="unit">Unit that should be checked when this event fires.</param>
        /// <param name="callback">Function to call when this event fires.</param>
        /// <param name="singleInstance">Whether or not to remove the event listener after calling the <paramref name="callback"/> function.</param>
        public void AddListener(object owner, IAttackableUnit unit, Action<IDamageData> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, IAttackableUnit, Action<IDamageData>, bool>(owner, unit, callback, singleInstance);
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

        public void Publish(IDamageData damageData)
        {
            var count = _listeners.Count;

            if (count == 0)
            {
                return;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2 == damageData.Attacker)
                {
                    var listener = _listeners[i];
                    listener.Item3(damageData);

                    if (listener.Item4)
                    {
                        _listeners.Remove(listener);
                    }
                }
            }
        }
    }

    public class EventOnDeath
    {
        private readonly List<Tuple<object, IAttackableUnit, Action<IDeathData>, bool>> _listeners = new List<Tuple<object, IAttackableUnit, Action<IDeathData>, bool>>();
        public void AddListener(object owner, IAttackableUnit target, Action<IDeathData> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, IAttackableUnit, Action<IDeathData>, bool>(owner, target, callback, singleInstance);
            _listeners.Add(listenerTuple);
        }
        public void RemoveListener(object owner)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner);
        }

        public void Publish(IDeathData deathData)
        {
            var count = _listeners.Count;

            if (count == 0)
            {
                return;
            }
            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2 == deathData.Unit)
                {
                    var listener = _listeners[i];
                    listener.Item3(deathData);
                    if (listener.Item4)
                    {
                        _listeners.Remove(listener);
                    }
                }
            }
        }
    }

    public class EventOnHitUnit
    {
        private readonly List<Tuple<object, IObjAiBase, Action<IDamageData>, bool>> _listeners = new List<Tuple<object, IObjAiBase, Action<IDamageData>, bool>>();
        public void AddListener(object owner, IObjAiBase unit, Action<IDamageData> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, IObjAiBase, Action<IDamageData>, bool>(owner, unit, callback, singleInstance);
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
        public void Publish(IObjAiBase unit, IDamageData data)
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
                    var listener = _listeners[i];
                    listener.Item3(data);
                    if (_listeners[i].Item4)
                    {
                        _listeners.Remove(listener);
                    }
                }
            }
        }
    }

    public class EventOnIncrementChampionScore
    {
        private readonly List<Tuple<object, IChampion, Action<IScoreData>, bool>> _listeners = new List<Tuple<object, IChampion, Action<IScoreData>, bool>>();

        public void AddListener(object owner, IChampion champion, Action<IScoreData> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, IChampion, Action<IScoreData>, bool>(owner, champion, callback, singleInstance);
            _listeners.Add(listenerTuple);
        }

        public void RemoveListener(object owner)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner);
        }

        public void Publish(IScoreData scoreData)
        {
            var count = _listeners.Count;

            if (count == 0)
            {
                return;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2 == scoreData.Owner)
                {
                    var listener = _listeners[i];
                    listener.Item3(scoreData);

                    if (listener.Item4)
                    {
                        _listeners.Remove(listener);
                    }
                }
            }
        }
    }

    public class EventOnKill
    {
        private readonly List<Tuple<object, IAttackableUnit, Action<IDeathData>, bool>> _listeners = new List<Tuple<object, IAttackableUnit, Action<IDeathData>, bool>>();

        public void AddListener(object owner, IAttackableUnit killer, Action<IDeathData> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, IAttackableUnit, Action<IDeathData>, bool>(owner, killer, callback, singleInstance);
            _listeners.Add(listenerTuple);
        }
        public void RemoveListener(object owner)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public void Publish(IDeathData deathData)
        {
            var count = _listeners.Count;

            if (count == 0)
            {
                return;
            }
            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2 == deathData.Killer)
                {
                    var listener = _listeners[i];
                    listener.Item3(deathData);

                    if (listener.Item4)
                    {
                        _listeners.Remove(listener);
                    }
                }
            }
        }
    }
    public class EventOnKillUnit
    {
        private readonly List<Tuple<object, IAttackableUnit, Action<IDeathData>, bool>> _listeners = new List<Tuple<object, IAttackableUnit, Action<IDeathData>, bool>>();

        public void AddListener(object owner, IAttackableUnit killer, Action<IDeathData> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, IAttackableUnit, Action<IDeathData>, bool>(owner, killer, callback, singleInstance);
            _listeners.Add(listenerTuple);
        }
        public void RemoveListener(object owner)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public void Publish(IDeathData deathData)
        {
            var count = _listeners.Count;

            if (count == 0)
            {
                return;
            }
            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2 == deathData.Killer)
                {
                    var listener = _listeners[i];
                    listener.Item3(deathData);

                    if (listener.Item4)
                    {
                        _listeners.Remove(listener);
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
                    var listener = _listeners[i];
                    listener.Item3(spell);

                    if (listener.Item4)
                    {
                        _listeners.Remove(listener);
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
                    var listener = _listeners[i];
                    listener.Item3(casterSpellPair.Value, missile);
                    
                    if (listener.Item4)
                    {
                        _listeners.Remove(listener);
                    }
                }
            }
        }
    }

    public class EventOnLevelUp
    {
        private readonly List<Tuple<object, IAttackableUnit, Action<IAttackableUnit>, bool>> _listeners = new List<Tuple<object, IAttackableUnit, Action<IAttackableUnit>, bool>>();

        public void AddListener(object owner, IAttackableUnit owner2, Action<IAttackableUnit> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, IAttackableUnit, Action<IAttackableUnit>, bool>(owner, owner2, callback, singleInstance);
            _listeners.Add(listenerTuple);
        }
        public void RemoveListener(object owner)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public void Publish(IAttackableUnit owner)
        {
            var count = _listeners.Count;

            if (count == 0)
            {
                return;
            }
            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2 == owner)
                {
                    var listener = _listeners[i];
                    listener.Item3(owner);

                    if (listener.Item4)
                    {
                        _listeners.Remove(listener);
                    }
                }
            }
        }
    }
    public class EventOnLevelUpSpell
    {
        private readonly List<Tuple<object, ISpell, Action<ISpell>, bool>> _listeners = new List<Tuple<object, ISpell, Action<ISpell>, bool>>();

        public void AddListener(object owner, ISpell spell, Action<ISpell> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, ISpell, Action<ISpell>, bool>(owner, spell, callback, singleInstance);
            _listeners.Add(listenerTuple);
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
                    var listener = _listeners[i];
                    listener.Item3(spell);
                    
                    if (listener.Item4)
                    {
                        _listeners.Remove(listener);
                    }
                }
            }
        }
    }

    public class EventOnMoveEnd
    {
        private readonly List<Tuple<object, IAttackableUnit, Action<IAttackableUnit>, bool>> _listeners = new List<Tuple<object, IAttackableUnit, Action<IAttackableUnit>, bool>>();
        public void AddListener(object owner, IAttackableUnit unit, Action<IAttackableUnit> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, IAttackableUnit, Action<IAttackableUnit>, bool>(owner, unit, callback, singleInstance);
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
            var count = _listeners.Count;

            if (count == 0)
            {
                return;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2 == unit)
                {
                    var listener = _listeners[i];
                    listener.Item3(unit);

                    if (listener.Item4)
                    {
                        _listeners.Remove(listener);
                    }
                }
            }
        }
    }

    public class EventOnMoveFailure
    {
        private readonly List<Tuple<object, IAttackableUnit, Action<IAttackableUnit>, bool>> _listeners = new List<Tuple<object, IAttackableUnit, Action<IAttackableUnit>, bool>>();
        public void AddListener(object owner, IAttackableUnit unit, Action<IAttackableUnit> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, IAttackableUnit, Action<IAttackableUnit>, bool>(owner, unit, callback, singleInstance);
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
            var count = _listeners.Count;

            if (count == 0)
            {
                return;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2 == unit)
                {
                    var listener = _listeners[i];
                    listener.Item3(unit);

                    if (listener.Item4)
                    {
                        _listeners.Remove(listener);
                    }
                }
            }
        }
    }

    public class EventOnMoveSuccess
    {
        private readonly List<Tuple<object, IAttackableUnit, Action<IAttackableUnit>, bool>> _listeners = new List<Tuple<object, IAttackableUnit, Action<IAttackableUnit>, bool>>();
        public void AddListener(object owner, IAttackableUnit unit, Action<IAttackableUnit> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, IAttackableUnit, Action<IAttackableUnit>, bool>(owner, unit, callback, singleInstance);
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
            var count = _listeners.Count;

            if (count == 0)
            {
                return;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2 == unit)
                {
                    var listener = _listeners[i];
                    listener.Item3(unit);
                    
                    if (listener.Item4)
                    {
                        _listeners.Remove(listener);
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
                    var listener = _listeners[i];
                    listener.Item3(spell);

                    if (listener.Item4)
                    {
                        _listeners.Remove(listener);
                    }
                }
            }
        }
    }

    public class EventOnPreDealDamage
    {
        private readonly List<Tuple<object, IAttackableUnit, Action<IDamageData>, bool>> _listeners = new List<Tuple<object, IAttackableUnit, Action<IDamageData>, bool>>();
        public void AddListener(object owner, IAttackableUnit unit, Action<IDamageData> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, IAttackableUnit, Action<IDamageData>, bool>(owner, unit, callback, singleInstance);
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
        public void Publish(IDamageData damageData)
        {
            var count = _listeners.Count;

            if (count == 0)
            {
                return;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2 == damageData.Attacker)
                {
                    var listener = _listeners[i];
                    listener.Item3(damageData);
                    if (listener.Item4)
                    {
                        _listeners.Remove(listener);
                    }
                }
            }
        }
    }

    public class EventOnPreTakeDamage
    {
        private readonly List<Tuple<object, IAttackableUnit, Action<IDamageData>, bool>> _listeners = new List<Tuple<object, IAttackableUnit, Action<IDamageData>, bool>>();
        public void AddListener(object owner, IAttackableUnit unit, Action<IDamageData> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, IAttackableUnit, Action<IDamageData>, bool>(owner, unit, callback, singleInstance);
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
        public void Publish(IDamageData damageData)
        {
            var count = _listeners.Count;

            if (count == 0)
            {
                return;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2 == damageData.Target)
                {
                    var listener = _listeners[i];
                    listener.Item3(damageData);

                    if (listener.Item4)
                    {
                        _listeners.Remove(listener);
                    }
                }
            }
        }
    }

    public class EventOnResurrect
    {
        private readonly List<Tuple<object, IObjAiBase, Action<IObjAiBase>, bool>> _listeners = new List<Tuple<object, IObjAiBase, Action<IObjAiBase>, bool>>();
        public void AddListener(object owner, IObjAiBase unit, Action<IObjAiBase> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, IObjAiBase, Action<IObjAiBase>, bool>(owner, unit, callback, singleInstance);
            _listeners.Add(listenerTuple);
        }
        public void RemoveListener(object owner)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public void Publish(IObjAiBase unit)
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
                    var listener = _listeners[i];
                    listener.Item3(unit);

                    if (listener.Item4)
                    {
                        _listeners.Remove(listener);
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
        private readonly List<Tuple<object, ISpell, Action<ISpell, ChannelingStopSource>>> _listeners = new List<Tuple<object, ISpell, Action<ISpell, ChannelingStopSource>>>();
        public void AddListener(object owner, ISpell spell, Action<ISpell, ChannelingStopSource> callback)
        {
            var listenerTuple = new Tuple<object, ISpell, Action<ISpell, ChannelingStopSource>>(owner, spell, callback);
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
        public void Publish(ISpell spell, ChannelingStopSource reason)
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
                    _listeners[i].Item3(spell, reason);
                }
            }
        }
    }

    public class EventOnSpellHit
    {
        private readonly List<Tuple<object, ISpell, Action<ISpell, IAttackableUnit, ISpellMissile, ISpellSector>, bool>> _listeners = new List<Tuple<object, ISpell, Action<ISpell, IAttackableUnit, ISpellMissile, ISpellSector>, bool>>();
        public void AddListener(object owner, ISpell spell, Action<ISpell, IAttackableUnit, ISpellMissile, ISpellSector> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, ISpell, Action<ISpell, IAttackableUnit, ISpellMissile, ISpellSector>, bool>(owner, spell, callback, singleInstance);
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
        public void Publish(ISpell spell, IAttackableUnit target, ISpellMissile p, ISpellSector s)
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
                    var listener = _listeners[i];
                    listener.Item3(spell, target, p, s);

                    if (listener.Item4)
                    {
                        _listeners.Remove(listener);
                    }
                }
            }
        }
    }

    public class EventOnSpellMissileHit
    {
        private readonly List<Tuple<object, ISpellMissile, Action<IAttackableUnit, ISpellMissile>, bool>> _listeners = new List<Tuple<object, ISpellMissile, Action<IAttackableUnit, ISpellMissile>, bool>>();
        public void AddListener(object owner, ISpellMissile missile, Action<IAttackableUnit, ISpellMissile> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, ISpellMissile, Action<IAttackableUnit, ISpellMissile>, bool>(owner, missile, callback, singleInstance);
            _listeners.Add(listenerTuple);
        }
        public void RemoveListener(object owner, ISpellMissile missile)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner && listener.Item2 == missile);
        }
        public void RemoveListener(object owner)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public void Publish(IAttackableUnit target, ISpellMissile m)
        {
            var count = _listeners.Count;

            if (count == 0)
            {
                return;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2 == m)
                {
                    _listeners[i].Item3(target, m);
                    if (_listeners[i].Item4)
                    {
                        _listeners.RemoveAt(i);
                    }
                }
            }
        }
    }

    public class EventOnSpellMissileEnd
    {
        private readonly List<Tuple<object, ISpellMissile, Action<ISpellMissile>, bool>> _listeners = new List<Tuple<object, ISpellMissile, Action<ISpellMissile>, bool>>();
        public void AddListener(object owner, ISpellMissile missile, Action<ISpellMissile> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, ISpellMissile, Action<ISpellMissile>, bool>(owner, missile, callback, singleInstance);
            _listeners.Add(listenerTuple);
        }
        public void RemoveListener(object owner, ISpellMissile missile)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner && listener.Item2 == missile);
        }
        public void RemoveListener(object owner)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public void Publish(ISpellMissile m)
        {
            var count = _listeners.Count;

            if (count == 0)
            {
                return;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2 == m)
                {
                    _listeners[i].Item3(m);
                    if (_listeners[i].Item4)
                    {
                        _listeners.RemoveAt(i);
                    }
                }
            }
        }
    }

    public class EventOnSpellMissileUpdate
    {
        private readonly List<Tuple<object, ISpellMissile, Action<ISpellMissile, float>, bool>> _listeners = new List<Tuple<object, ISpellMissile, Action<ISpellMissile, float>, bool>>();

        public void AddListener(object owner, ISpellMissile missile, Action<ISpellMissile, float> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, ISpellMissile, Action<ISpellMissile, float>, bool>(owner, missile, callback, singleInstance);
            _listeners.Add(listenerTuple);
        }
        public void RemoveListener(object owner)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public void Publish(ISpellMissile missile, float diff)
        {
            var count = _listeners.Count;

            if (count == 0)
            {
                return;
            }
            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2 == missile)
                {
                    var listener = _listeners[i];
                    listener.Item3(missile, diff);

                    if (listener.Item4)
                    {
                        _listeners.Remove(listener);
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

    public class EventOnSpellSectorHit
    {
        private readonly List<Tuple<object, ISpellSector, Action<IAttackableUnit, ISpellSector>, bool>> _listeners = new List<Tuple<object, ISpellSector, Action<IAttackableUnit, ISpellSector>, bool>>();
        public void AddListener(object owner, ISpellSector sector, Action<IAttackableUnit, ISpellSector> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, ISpellSector, Action<IAttackableUnit, ISpellSector>, bool>(owner, sector, callback, singleInstance);
            _listeners.Add(listenerTuple);
        }
        public void RemoveListener(object owner, ISpellSector sector)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner && listener.Item2 == sector);
        }
        public void RemoveListener(object owner)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public void Publish(IAttackableUnit target, ISpellSector s)
        {
            var count = _listeners.Count;

            if (count == 0)
            {
                return;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2 == s)
                {
                    _listeners[i].Item3(target, s);
                    if (_listeners[i].Item4)
                    {
                        _listeners.RemoveAt(i);
                    }
                }
            }
        }
    }

    public class EventOnTakeDamage
    {
        private readonly List<Tuple<object, IAttackableUnit, Action<IDamageData>, bool>> _listeners = new List<Tuple<object, IAttackableUnit, Action<IDamageData>, bool>>();
        /// <summary>
        /// Adds a listener for this event, wherein, if the unit that took damage was the given unit, it will call the <paramref name="callback"/> function.
        /// </summary>
        /// <param name="owner">Object which will own this listener. Used in removal. Often times "this" will suffice.</param>
        /// <param name="unit">Unit that should be checked when this event fires.</param>
        /// <param name="callback">Function to call when this event fires.</param>
        /// <param name="singleInstance">Whether or not to remove the event listener after calling the <paramref name="callback"/> function.</param>
        public void AddListener(object owner, IAttackableUnit unit, Action<IDamageData> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, IAttackableUnit, Action<IDamageData>, bool>(owner, unit, callback, singleInstance);
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

        public void Publish(IDamageData damageData)
        {
            var count = _listeners.Count;

            if (count == 0)
            {
                return;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2 == damageData.Target)
                {
                    var listener = _listeners[i];
                    listener.Item3(damageData);

                    if (listener.Item4)
                    {
                        _listeners.Remove(listener);
                    }
                }
            }
        }
    }

    public class EventOnTargetLost
    {
        private readonly List<Tuple<object, IObjAiBase, Action<IAttackableUnit>, bool>> _listeners = new List<Tuple<object, IObjAiBase, Action<IAttackableUnit>, bool>>();
        /// <summary>
        /// Adds a listener for this event, wherein, if the unit that lost their target was the given unit, it will call the <paramref name="callback"/> function.
        /// </summary>
        /// <param name="owner">Object which will own this listener. Used in removal. Often times "this" will suffice.</param>
        /// <param name="unit">Unit that should be checked when this event fires.</param>
        /// <param name="callback">Function to call when this event fires.</param>
        /// <param name="singleInstance">Whether or not to remove the event listener after calling the <paramref name="callback"/> function.</param>
        public void AddListener(object owner, IObjAiBase unit, Action<IAttackableUnit> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, IObjAiBase, Action<IAttackableUnit>, bool>(owner, unit, callback, singleInstance);
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

        public void Publish(IObjAiBase unit, IAttackableUnit prevTarget)
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
                    var listener = _listeners[i];
                    listener.Item3(prevTarget);

                    if (listener.Item4)
                    {
                        _listeners.Remove(listener);
                    }
                }
            }
        }
    }

    public class EventOnUnitBuffDeactivated
    {
        private readonly List<Tuple<object, IAttackableUnit, Action<IBuff>, bool>> _listeners = new List<Tuple<object, IAttackableUnit, Action<IBuff>, bool>>();
        public void AddListener(object owner, IAttackableUnit unit, Action<IBuff> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, IAttackableUnit, Action<IBuff>, bool>(owner, unit, callback, singleInstance);
            _listeners.Add(listenerTuple);
        }
        public void RemoveListener(object owner)
        {
            _listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public void Publish(IBuff buff, IAttackableUnit unit)
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
                    var listener = _listeners[i];
                    listener.Item3(buff);

                    if (listener.Item4)
                    {
                        _listeners.Remove(listener);
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
        private readonly List<Tuple<object, IObjAiBase, Func<IObjAiBase, OrderType, bool>, bool>> _listeners = new List<Tuple<object, IObjAiBase, Func<IObjAiBase, OrderType, bool>, bool>>();
        public void AddListener(object owner, IObjAiBase unit, Func<IObjAiBase, OrderType, bool> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, IObjAiBase, Func<IObjAiBase, OrderType, bool>, bool>(owner, unit, callback, singleInstance);
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

        public bool Publish(IObjAiBase unit, OrderType order)
        {
            bool returnVal = true;

            var count = _listeners.Count;

            if (count == 0)
            {
                return returnVal;
            }

            for (int i = count - 1; i >= 0; i--)
            {
                if (_listeners[i].Item2 == unit)
                {
                    var listener = _listeners[i];
                    returnVal = listener.Item3(unit, order);

                    if (listener.Item4)
                    {
                        _listeners.RemoveAt(i);
                    }
                }
            }

            return returnVal;
        }
    }

    public class EventOnUpdateStats
    {
        private readonly List<Tuple<object, IAttackableUnit, Action<IAttackableUnit, float>, bool>> _listeners = new List<Tuple<object, IAttackableUnit, Action<IAttackableUnit, float>, bool>>();
        public void AddListener(object owner, IAttackableUnit unit, Action<IAttackableUnit, float> callback, bool singleInstance)
        {
            var listenerTuple = new Tuple<object, IAttackableUnit, Action<IAttackableUnit, float>, bool>(owner, unit, callback, singleInstance);
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

        public void Publish(IAttackableUnit unit, float diff)
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
                    _listeners[i].Item3(unit, diff);
                    if (_listeners[i].Item4)
                    {
                        _listeners.RemoveAt(i);
                    }
                }
            }
        }
    }
}
