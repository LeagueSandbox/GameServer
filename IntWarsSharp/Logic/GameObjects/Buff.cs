using IntWarsSharp.Logic.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntWarsSharp.Logic.GameObjects
{
    public enum BuffType
    {
        BUFFTYPE_ETERNAL,
        BUFFTYPE_TEMPORARY
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

        public void setName(string name)
        {
            this.name = name;
        }


        public bool needsToRemove()
        {
            return remove;
        }
        public Buff(string buffName, float dur, BuffType type, Unit u, Unit attacker)
        {
            this.duration = dur;
            this.name = buffName;
            this.timeElapsed = 0;
            this.remove = false;
            this.attachedTo = u;
            this.attacker = attacker;
            this.buffType = type;
            this.movementSpeedPercentModifier = 0.0f;
        }
        public Buff(string buffName, float dur, BuffType type, Unit u) //no attacker specified = selfbuff, attacker aka source is same as attachedto
        {
            this.duration = dur;
            this.name = buffName;
            this.timeElapsed = 0;
            this.remove = false;
            this.attachedTo = u;
            this.attacker = u;
            this.buffType = type;
            this.movementSpeedPercentModifier = 0.0f;
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
            timeElapsed += (float)diff / 1000000.0f;

            //Fuck LUA
            /*  if (buffScript != null && buffScript.isLoaded())
              {
                  buffScript.lua.get<sol::function>("onUpdate").call<void>(diff);
              }*/

            if (getBuffType() != BuffType.BUFFTYPE_ETERNAL)
            {
                if (timeElapsed >= duration)
                {
                    if (name != "")
                    { // empty name = no buff icon
                        PacketNotifier.notifyRemoveBuff(attachedTo, name);
                    }
                    remove = true;
                }
            }
        }

    }
}
