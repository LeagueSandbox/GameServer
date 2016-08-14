Here is all Lua functions available right now

# Global functions
```lua
-- Set the model of the champion
setChampionModel(champion, modelName)
-- Teleport to the location
teleportTo(unit, x, y)
-- Add a particle from a champion to position x,y
addParticle(champion, particleName, x, y)
-- Attach particle from a champion to a target
addParticleTarget(champion, particleName, target)
-- Add a buff with name with a duration (0 if infinite) with the receiver of the buff (onto) and the buff giver (from)
addBuff(buffName, duration, onto, from)
-- Get the number of stacks of a buff on a unit
getStacks(buffName, target)
-- Set the number of stacks of a buff on a unit
setStacks(buffName, target, stacks)
-- Print a message in the chat for everybody
printChat(msg)
-- Get all units in a range from a target
getUnitsInRange(target, range, isAlive)
-- Get all champions in a range from a target
getChampionsInRange(target, range, isAlive)
-- Dash to x,y with the a speed
dashTo(unit, x, y, dashSpeed)
-- Get team of the object
getTeam(gameObject)
-- Get if the unit is dead
isDead(unit)
-- Set the amount of gold of the champion
setGold(champion, amount)
-- Add an amount of gold to a champion
addGold(champion, amount)
```

# Buff specific
```lua
function onAddBuff()
-- This function is called while the buff is added
end

function onUpdate(diff)
-- This function is called each frame with a delta time in ms
end

function onBuffEnd()
-- This function is called when the buff ends
end

-- Get the giver of the buff
getSourceUnit()
-- Get the unit whom has the buff
getUnit()
```

# Unit specific
```lua
none
```

# Spell specific
```lua
getOwner()
getOwnerX()
getOwnerY()
getSpellLevel()
getOwnerLevel()
getChampionModel()
getCastTarget()
getSpellToX()
getSpellToY()
getRange()
getProjectileSpeed()
getCoefficient()
addProjectile(projectileName, toX, toY)
addProjectile(projectileName, target)
getEffectValue(effectNo)
destroyProjectile()
getTarget()
dealPhysicalDamage(amount)
dealMagicalDamage(amount)
dealTrueDamage(amount)
getNumberObjectsHit()
```