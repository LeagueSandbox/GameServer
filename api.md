Here all function now avaible is Lua

# Global functions
```lua
setChampionModel(champion, modelName)
teleportTo(unit, x, y)
addParticle(champion, particleName, x, y)
addParticleTarget(champion, particleName, target)
addBuff(buffName, duration, stacks, onto, from)
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
modifyStats(stat, valueToAdd) 
```

# Avaible stats
```lua
crit
armor
mr
hp5
mp5
range
adFlat
adPct
apFlat
apPct
attackSpeed
cdr
armorPenFlat
armorPenPct
lifeSteal
spellVamp
tenacity
maxHp
maxMp
moveSpeed
size
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
getNumberObjectsHit()
```