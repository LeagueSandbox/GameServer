using LeagueSandbox.GameServer.Logic.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    /*public enum BuffType
    {
        BUFFTYPE_ETERNAL,
        BUFFTYPE_TEMPORARY
    }*/

    public enum BuffType : byte
    {
        Internal,
        Aura,
        CombatEnchancer,
        CombatDehancer,
        SpellShield,
        Stun,
        Invisibility,
        Silence,
        Taunt,
        Polymorph,
        Slow,
        Snare,
        Damage,
        Heal,
        Haste,
        SpellImmunity,
        PhysicalImmunity,
        Invulnerability,
        Sleep,
        NearSight,
        Frenzy,
        Fear,
        Charm,
        Poison,
        Suppression,
        Blind,
        Counter,
        Shred,
        Flee,
        Knockup,
        Knockback,
        Disarm
    }

    public class Buff
    {
        protected float duration;
        protected float movementSpeedPercentModifier;
        protected float timeElapsed;
        protected bool remove;
        protected Unit attachedTo;
        protected Unit attacker; // who added this buff to the unit it's attached to
        protected BuffType buffType;
        protected LuaScript buffScript;
        protected string name;
        protected int stacks;

        protected virtual void init()
        {

        }

        public BuffType getBuffType()
        {
            return buffType;
        }

        public Unit getUnit()
        {
            return attachedTo;
        }

        public Unit getSourceUnit()
        {
            return attacker;
        }

        public void setName(string name)
        {
            this.name = name;
        }


        public bool needsToRemove()
        {
            return remove;
        }
        public Buff(string buffName, float dur, int stacks, Unit u, Unit attacker)
        {
            this.duration = dur;
            this.stacks = stacks;
            this.name = buffName;
            this.timeElapsed = 0;
            this.remove = false;
            this.attachedTo = u;
            this.attacker = attacker;
            this.buffType = BuffType.Aura;
            this.movementSpeedPercentModifier = 0.0f;
            attachedTo.GetGame().GetPacketNotifier().notifyAddBuff(this);
        }
        public Buff(string buffName, float dur, Unit u) //no attacker specified = selfbuff, attacker aka source is same as attachedto
        {
            this.duration = dur;
            this.name = buffName;
            this.timeElapsed = 0;
            this.remove = false;
            this.attachedTo = u;
            this.attacker = u;
            this.buffType = BuffType.Aura;
            this.movementSpeedPercentModifier = 0.0f;
            attachedTo.GetGame().GetPacketNotifier().notifyAddBuff(this);
        }

        public float getMovementSpeedPercentModifier()
        {
            return movementSpeedPercentModifier;
        }

        public void setMovementSpeedPercentModifier(float modifier)
        {
            movementSpeedPercentModifier = modifier;
        }

        public string getName()
        {
            return name;
        }

        public void setTimeElapsed(float time)
        {
            timeElapsed = time;
        }

        public void update(long diff)
        {
            timeElapsed += (float)diff / 1000.0f;

            //Fuck LUA
            /*  if (buffScript != null && buffScript.isLoaded())
              {
                  buffScript.lua.get<sol::function>("onUpdate").call<void>(diff);
              }*/

            if (duration != 0.0f)
            {
                if (timeElapsed >= duration)
                {
                    if (name != "")
                    { // empty name = no buff icon
                        attachedTo.GetGame().GetPacketNotifier().notifyRemoveBuff(attachedTo, name);
                    }
                    remove = true;
                }
            }
        }

        public int getStacks()
        {
            return stacks;
        }
    }
}
