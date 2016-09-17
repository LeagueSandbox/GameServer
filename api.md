Here is all lua functions available right now

# Global functions
```lua
-- Changes champion model
setChampionModel(champion, modelName)
-- teleports unit to the desired position
teleportTo(unit, x, y)
-- adds particle to the position
addParticle(champion, particleName, x, y, size, boneName)
-- adds particle to the desired unit's desired buffbone
addParticleTarget(champion, particleName, target, size, boneName)
-- adds buff to the desired unit
addBuff(buffName, duration, stacks, onto, from)
-- prints a message in chat
printChat(msg)
-- returns units in range
getUnitsInRange(target, range, isAlive)
-- returns champions in range
getChampionsInRange(target, range, isAlive)
-- dashes the desired unit to desired position
dashTo(unit, x, y, dashSpeed, height, animName)
-- returns the team of the object
getTeam(gameObject)
-- returns true if unit is dead
isDead(unit)
-- some advanced stuff xD
sendPacket(packet)
-- will return true if unit is champ/minion etc.
unitIsChampion(unit)
unitIsMinion(unit)
unitIsTurret(unit)
unitIsInhibitor(unit)
unitIsNexus(unit)
unitIsPlaceable(unit)
unitIsMonster(unit)
```

# Buff specific
```lua
getSourceUnit()
getUnit()
```

# Unit specific
```lua
-- none
```

# Champion passive specific
```lua
function onUpdate(diff)
    -- called each diff
end

function onDamageTaken(attacker, damage, type, source)
    -- will be called as you take damage, also you can use this:
    modifyIncomingDamage(toAmount) -- to change the damage will be dealt to you.
end

function onAutoAttack(target)
    -- called each time you auto attacker
end

function onDealDamage(target, damage, type, source)
    -- called each time you deal damage.
end

function onSpellCast(x, y, slot, target)
    -- called as you cast spell.
end

function onDie(killer)
    -- called as you die
end
```

# Spell specific
```lua

function onStartCasting()
    -- Called on cast
end

function onFinishCasting()
    -- Called as cast time finishes
end

function onUpdate(diff)
    -- Called every diff.
end

function applyEffects()
    --Called when projectiles hit something
end

owner -- champion you use
owner.X
owner.Y
spellLevel
getOwnerLevel()
getChampionModel()
castTarget
spell.X
spell.Y
getProjectileSpeed()
getCoefficient(coeffNo)
addProjectile(projectileName, toX, toY, isServerProjectile)
addProjectileTarget(projectileName, target, isServerProjectile)
addProjectileCustom(projectileName, fromX, fromY, toX, toY, isServerProjectile)
addProjectileCustomTarget(projectileName, fromX, fromY, target, isServerProjectile)
addPlaceable(toX, toY, model, name)
getEffectValue(effectNo)
destroyProjectile()
getTarget()
dealPhysicalDamage(amount)
dealMagicalDamage(amount)
dealTrueDamage(amount)
getNumberObjectsHit()
```
