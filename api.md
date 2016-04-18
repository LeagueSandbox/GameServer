Here all function now avaible is Lua

# Global functions
```lua
setChampionModel(champion, modelName)
teleportTo(unit, x, y)
addParticle(champion, particleName, x, y)
addParticleTarget(champion, particleName, target)
addBuff(buffName, duration, onto, from)
printChat(msg)
getUnitsInRange(target, range, isAlive)
getChampionsInRange(target, range, isAlive)
dashTo(unit, x, y, dashSpeed)
getTeam(gameObject)
isDead(unit)
```

# Buff specific
```lua
getSourceUnit()
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
```