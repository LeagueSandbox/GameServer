using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;

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
        public static EventOnLevelUpSpell OnLevelUpSpell = new EventOnLevelUpSpell();
        public static EventOnLevelUp OnLevelUp = new EventOnLevelUp();
        public static EventOnChampionDamageTaken OnChampionDamageTaken = new EventOnChampionDamageTaken();
        public static EventOnChampionDamageDealt OnChampionDamageDealt = new EventOnChampionDamageDealt();
        public static EventOnUnitDamageTaken OnUnitDamageTaken = new EventOnUnitDamageTaken();
        public static EventOnUnitDamageDealt OnUnitDamageDealt = new EventOnUnitDamageDealt();
        public static EventOnAutoAttackHit OnAutoAttackHit = new EventOnAutoAttackHit();
        public static EventOnSpellHit OnSpellHit = new EventOnSpellHit();
        public static EventOnMoveSuccess OnMoveSuccess = new EventOnMoveSuccess();
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
        private List<Tuple<object, AttackableUnit, Action>> listeners = new List<Tuple<object, AttackableUnit, Action>>();
        public void AddListener(object owner, AttackableUnit unit, Action callback)
        {
            var listenerTuple = new Tuple<object, AttackableUnit, Action>(owner, unit, callback);
            listeners.Add(listenerTuple);
        }

        public void RemoveListener(object owner, AttackableUnit unit)
        {
            listeners.RemoveAll((listener) => listener.Item1 == owner && listener.Item2 == unit);
        }
        public void RemoveListener(object owner)
        {
            listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public void Publish(AttackableUnit unit)
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

    public class EventOnUnitDamageDealt
    {
        private List<Tuple<object, AttackableUnit, Action<AttackableUnit>>> listeners = new List<Tuple<object, AttackableUnit, Action<AttackableUnit>>>();
        public void AddListener(object owner, AttackableUnit unit, Action<AttackableUnit> callback)
        {
            var listenerTuple = new Tuple<object, AttackableUnit, Action<AttackableUnit>>(owner, unit, callback);
            listeners.Add(listenerTuple);
        }

        public void RemoveListener(object owner, AttackableUnit unit)
        {
            listeners.RemoveAll((listener) => listener.Item1 == owner && listener.Item2 == unit);
        }
        public void RemoveListener(object owner)
        {
            listeners.RemoveAll((listener) => listener.Item1 == owner);
        }
        public void Publish(AttackableUnit attacker, AttackableUnit target)
        {
            listeners.ForEach((listener) => {
                listener.Item3(target);
            });
            if (target is Champion)
            {
                ApiEventManager.OnChampionDamageDealt.Publish(attacker,(Champion)target);
            }
        }
    }

    public class EventOnLevelUpSpell
    {
        private List<Tuple<object, Spell, Action<Champion>>> listeners = new List<Tuple<object, Spell, Action<Champion>>>();

        public void AddListener(object owner, Spell spell, Action<Champion> callback)
        {
            var listenerTuple = new Tuple<object, Spell, Action<Champion>>(owner, spell, callback);
            listeners.Add(listenerTuple);
        }

        public void RemoveListener(object owner, Spell spell)
        {
            listeners.RemoveAll((listener) => listener.Item1 == owner && listener.Item2 == spell);
        }

        public void RemoveListener(object owner)
        {
            listeners.RemoveAll((listener) => listener.Item1 == owner);
        }

        public void Publish(Spell spell, Champion champion)
        {
            listeners.ForEach((listener) =>
            {
                if (listener.Item2 == spell)
                {
                    listener.Item3(champion);
                }
            });
        }
    }

    public class EventOnLevelUp
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

    public class EventOnChampionDamageDealt
    {
        private List<Tuple<object, Champion, Action<Champion>>> listeners = new List<Tuple<object, Champion, Action<Champion>>>();
        public void AddListener(object owner, Champion champion, Action<Champion> callback)
        {
            var listenerTuple = new Tuple<object, Champion, Action<Champion>>(owner, champion, callback);
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

        public void Publish(AttackableUnit attacker, Champion target)
        {
            listeners.ForEach((listener) => {
                if (listener.Item2 == attacker)
                {
                    listener.Item3(target);
                }
            });
        }
    }

    public class EventOnAutoAttackHit
    {
        private List<Tuple<object, ObjAIBase, Action<AttackableUnit,bool>>> listeners = new List<Tuple<object, ObjAIBase, Action<AttackableUnit,bool>>>();

        public void AddListener(object owner, ObjAIBase unit, Action<AttackableUnit,bool> callback)
        {
            var listenerTuple = new Tuple<object, ObjAIBase, Action<AttackableUnit,bool>>(owner, unit, callback);
            listeners.Add(listenerTuple);
        }

        public void RemoveListener(object owner, ObjAIBase unit)
        {
            listeners.RemoveAll((listener) => listener.Item1 == owner && listener.Item2 == unit);
        }

        public void RemoveListener(object owner)
        {
            listeners.RemoveAll((listener) => listener.Item1 == owner);
        }

        public void Publish(ObjAIBase unit, AttackableUnit target, bool isCrit)
        {
            listeners.ForEach((listener) => {
                if (listener.Item2 == unit)
                {
                    listener.Item3(target, isCrit);
                }
            });
        }
    }

    public class EventOnSpellHit
    {
        private List<Tuple<Action<AttackableUnit, Spell>>> listeners = new List<Tuple<Action<AttackableUnit, Spell>>>();
        public void AddListener(Action<AttackableUnit, Spell> callback)
        {
            var listenerTuple = new Tuple<Action<AttackableUnit, Spell>>(callback);
            listeners.Add(listenerTuple);
        }

        public void RemoveListener(Action<AttackableUnit, Spell> callback)
        {
            listeners.RemoveAll((listener) => listener.Item1 == callback);
        }
        public void Publish(AttackableUnit target, Spell spell)
        {
            listeners.ForEach((listener) => {
                listener.Item1(target, spell);
            });
        }
    }

    public class EventOnMoveSuccess
    {
        private List<Tuple<Action<Champion, float, float>>> listeners = new List<Tuple<Action<Champion, float, float>>>();
        public void AddListener(Action<Champion, float, float> callback)
        {
            var listenerTuple = new Tuple<Action<Champion, float, float>>(callback);
            listeners.Add(listenerTuple);
        }

        public void RemoveListener(Action<Champion, float, float> callback)
        {
            listeners.RemoveAll((listener) => listener.Item1 == callback);
        }
        public void Publish(Champion movingChampion, float x, float y)
        {
            listeners.ForEach((listener) => {
                listener.Item1(movingChampion, x, y);
            });
        }
    }
}
