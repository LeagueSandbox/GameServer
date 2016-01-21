using InibinSharp;
using IntWarsSharp.Core.Logic;
using IntWarsSharp.Core.Logic.RAF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntWarsSharp.Logic.GameObjects
{
    enum SpellFlag : uint
    {
        SPELL_FLAG_AutoCast = 0x00000002,
        SPELL_FLAG_InstantCast = 0x00000004,
        SPELL_FLAG_PersistThroughDeath = 0x00000008,
        SPELL_FLAG_NonDispellable = 0x00000010,
        SPELL_FLAG_NoClick = 0x00000020,
        SPELL_FLAG_AffectImportantBotTargets = 0x00000040,
        SPELL_FLAG_AllowWhileTaunted = 0x00000080,
        SPELL_FLAG_NotAffectZombie = 0x00000100,
        SPELL_FLAG_AffectUntargetable = 0x00000200,
        SPELL_FLAG_AffectEnemies = 0x00000400,
        SPELL_FLAG_AffectFriends = 0x00000800,
        SPELL_FLAG_AffectBuildings = 0x00001000,
        SPELL_FLAG_NotAffectSelf = 0x00002000,
        SPELL_FLAG_AffectNeutral = 0x00004000,
        SPELL_FLAG_AffectAllSides = 0x00004C00,
        SPELL_FLAG_AffectMinions = 0x00008000,
        SPELL_FLAG_AffectHeroes = 0x00010000,
        SPELL_FLAG_AffectTurrets = 0x00020000,
        SPELL_FLAG_AffectAllUnitTypes = 0x00038000,
        SPELL_FLAG_AlwaysSelf = 0x00040000,
        SPELL_FLAG_AffectDead = 0x00080000,
        SPELL_FLAG_AffectNotPet = 0x00100000,
        SPELL_FLAG_AffectBarracksOnly = 0x00200000,
        SPELL_FLAG_IgnoreVisibilityCheck = 0x00400000,
        SPELL_FLAG_NonTargetableAlly = 0x00800000,
        SPELL_FLAG_NonTargetableEnemy = 0x01000000,
        SPELL_FLAG_NonTargetableAll = 0x01800000,
        SPELL_FLAG_TargetableToAll = 0x02000000,
        SPELL_FLAG_AffectWards = 0x04000000,
        SPELL_FLAG_AffectUseable = 0x08000000,
        SPELL_FLAG_IgnoreAllyMinion = 0x10000000,
        SPELL_FLAG_IgnoreEnemyMinion = 0x20000000,
        SPELL_FLAG_IgnoreLaneMinion = 0x40000000,
        SPELL_FLAG_IgnoreClones = 0x80000000,
    };

    enum SpellState
    {
        STATE_READY,
        STATE_CASTING,
        STATE_COOLDOWN
    };

    enum SpellTargetType : int
    {
        TARGET_SELF = 0, // teemo W ; xin Q
        TARGET_UNIT = 1, // Taric E ; Annie Q ; teemo Q ; xin E
        TARGET_LOC_AOE = 2, // Lux E, Ziggs R
        TARGET_CONE = 3, // Annie W, Kass E
        TARGET_SELF_AOE = 4, // sivir R, Gangplanck E
        TARGET_LOC = 6, // Ez Q, W, E, R ; Mundo Q
        TARGET_LOC2 = 7  // Morg Q, Cait's Q -- These don't seem to have Missile inibins, and SpawnProjectile doesn't seem necessary to show the projectiles
    };
    class Spell
    {
        protected Champion owner;
        protected short level = 0;
        protected short slot;
        protected string spellName;
        protected float targetType;
        protected int flags = 0;
        protected float projectileFlags = 0.0f;

        protected float castTime = 0.0f;
        protected float castRange = 1000.0f;
        protected float projectileSpeed = 2000.0f;
        protected float lineWidth;
        protected float[] cooldown = new float[5] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f };
        protected float[] cost = new float[5] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f };

        // Warning : this value usually contains one of the "ad/ap" bonus coefficient, as seen in "deals 50 (+{coefficient}%) damage"
        // However, it may not be accurate and there's no way to tell whether it's the ad or ap bonus for hybrid spells
        // Sometimes, it is also stored as an effect value instead of the coefficient
        protected float coefficient;
        protected List<List<float>> effects;

        protected float range = 0;

        protected SpellState state = SpellState.STATE_READY;
        protected float currentCooldown = 0;
        protected float currentCastTime = 0;
        protected int futureProjNetId;
        protected int spellNetId;

        protected Unit target;
        protected float x, y;

        public Spell(Champion owner, string spellName, short slot)
        {
            this.owner = owner;
            this.spellName = spellName;
            this.slot = slot;

            Inibin inibin;
            if (!RAFManager.getInstance().readFile("DATA/Spells/" + spellName + ".inibin", out inibin))
            {
                if (!RAFManager.getInstance().readFile("DATA/Characters/" + owner.getType() + "/Spells/" + spellName + ".inibin", out inibin))
                {
                    if (!RAFManager.getInstance().readFile("DATA/Characters/" + owner.getType() + "/" + spellName + ".inibin", out inibin))
                    {
                        Logger.LogCoreError("Couldn't find spell stats for " + spellName);
                        return;
                    }
                }
            }

            var i = 0;
            // Generate cooldown values for each level of the spell
            for (i = 0; i < cooldown.Length; ++i)
            {
                cooldown[i] = inibin.GetValue<float>("SpellData", "Cooldown" + (i + 1));
            }

            castTime = ((1.0f + inibin.GetValue<float>("SpellData", "DelayCastOffsetPercent"))) / 2.0f;

            flags = inibin.GetValue<int>("SpellData", "Flags");
            castRange = inibin.GetValue<float>("SpellData", "CastRange");
            projectileSpeed = inibin.GetValue<float>("SpellData", "MissileSpeed");
            coefficient = inibin.GetValue<float>("SpellData", "Coefficient");
            lineWidth = inibin.GetValue<float>("SpellData", "LineWidth");

            i = 1;
            while (true)
            {
                string key = "Effect" + (0 + i) + "Level0Amount";
                if (inibin.GetValue<object>("SpellData", key) == null)
                    break;


                List<float> effectValues = new List<float>();
                for (var j = 0; j < 6; ++j)
                {
                    key = "Effect" + (0 + i) + "Level" + (0 + j) + "Amount";
                    effectValues.Add(inibin.GetValue<float>("SpellData", key));
                }

                effects.Add(effectValues);
                ++i;
            }

            targetType = (float)Math.Floor(inibin.GetValue<float>("SpellData", "TargettingType") + 0.5f);


            // This is starting to get ugly. How many more names / paths to go ?
            if (!RAFManager.getInstance().readFile("DATA/Spells/" + spellName + "Missile.inibin", out inibin))
            {
                if (!RAFManager.getInstance().readFile("DATA/Spells/" + spellName + "Mis.inibin", out inibin))
                {
                    if (!RAFManager.getInstance().readFile("DATA/Characters/" + owner.getType() + "/Spells/" + spellName + "Missile.inibin", out inibin))
                    {
                        if (!RAFManager.getInstance().readFile("DATA/Characters/" + owner.getType() + "/" + spellName + "Missile.inibin", out inibin))
                        {
                            if (!RAFManager.getInstance().readFile("DATA/Characters/" + owner.getType() + "/Spells/" + spellName + "Mis.inibin", out inibin))
                            {
                                if (!RAFManager.getInstance().readFile("DATA/Characters/" + owner.getType() + "/" + spellName + "Mis.inibin", out inibin))
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
            }

            castRange = inibin.GetValue<float>("SpellData", "CastRange");
            projectileSpeed = inibin.GetValue<float>("SpellData", "MissileSpeed");
            projectileFlags = inibin.GetValue<float>("SpellData", "Flags");
        }

        /**
         * Called when the character casts the spell
         */
        public virtual bool cast(float x, float y, Unit* u = 0, uint32 futureProjNetId = 0, uint32 spellNetId = 0);

        /**
         * Called when the spell is finished casting and we're supposed to do things
         * such as projectile spawning, etc.
         */
        public virtual void finishCasting();

        /**
         * Called every diff milliseconds to update the spell
         */
        public virtual void update(int64 diff);

        /**
         * Called by projectiles when they land / hit
         * In here we apply the effects : damage, buffs, debuffs...
         */
        public virtual void applyEffects(Unit* t, Projectile* p = 0);

        public Champion* getOwner() const { return owner; }

    /**
     * @return Spell's unique ID
     */
    public uint32 getId() const;
    public float getCastTime() const { return castTime; }

public std::string getStringForSlot();

/*
 * does spell effects in lua if defined.
 */
public void doLua();
public void loadLua(LuaScript& script);
public void reloadLua();

public void setSlot(int _slot)
{
    slot = _slot;
}

/**
 * TODO : Add in CDR % from champion's stat
 */
public float getCooldown() const { 
      return 0; // TODO: Remove this
      if(!level) {
         return 0;
      }
      return cooldown[level - 1];
   }
   
   /**
    * @return the mana/energy/health cost
    */
  public float getCost() const {
      return 0; // TODO: Remove this
      if(!level) {
         return 0;
      }
      return cost[level - 1];
   }
   
 public uint32 getFlags() const { return flags; }
   
 public uint8 getLevel() const {
      return level;
   }
   
 public virtual void levelUp()
{
    ++level;
}

public SpellState getState() const {
      return state;
   }
   
 public uint8 getSlot() const {
      return slot;
   }
    }
}
