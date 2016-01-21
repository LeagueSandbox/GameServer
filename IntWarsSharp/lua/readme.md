Lua API
==================

Functions available in finishCasting() for champions
------
Function Name            | Parameters                                               | Description
-------------------------|----------------------------------------------------------|-------------------------------------------------------------------------------------------------------
addMovementSpeedBuff     | target, float percentModifier, float duration            | Applies a movement speed buff to the target.
addParticle              | string particle, float toX, float toY                    | Adds a particle to the given coords
addParticleTarget        | string particle, target                                  | Adds a particle to the target
addProjectile            | float toX, float toY                                     | Adds a projectile (e.g Ezreal's Q)
addProjectileCustom      | string name, float projectileSpeed, float toX, float toY | Adds a custom projectile
getChampionModel         |                                                          | Returns caster champion's model
getEffectValue           |                                                          | 
getOwner                 |                                                          | Returns caster champion, to use as an argument for a function
getOwnerLevel            |                                                          | Returns caster champion's level
getOwnerX, getOwnerY     |                                                          | Return caster champion's coords
getProjectileSpeed       |                                                          | 
getRange                 |                                                          | Returns spell range
getSide                  | object                                                   | Returns the side of the object
getSpellLevel            |                                                          | Returns spell level
getSpellToX, getSpellToY |                                                          | Spell casted to (NOTE: you must calculate range using vectors if you don't want unlimited spell range)
isDead                   | unit                                                     | Return true if the unit is dead
setChampionModel         | string newModel                                          | Sets caster champion's model
teleportTo               | float x, float y                                         | Teleports the champion to specified coords

Functions available in applyEffects()
------
Function Name      | Parameters   | Description
-------------------|--------------|------------------------
dealMagicalDamage  | float amount | Deals magical damage
dealPhysicalDamage | float amount | Deals physical damage
destroyProjectile  |              | Destroys the projectile
getTarget          |              | Returns target unit
