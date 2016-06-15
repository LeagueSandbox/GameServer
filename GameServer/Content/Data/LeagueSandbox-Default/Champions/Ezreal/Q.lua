Vector2 = require 'Vector2' -- include 2d vector lib 

function onFinishCasting()
    local current = Vector2:new(getOwnerX(), getOwnerY())
    local to = (Vector2:new(getSpellToX(), getSpellToY()) - current):normalize()
    local range = to * 1150
    local trueCoords = current + range

    addProjectile("EzrealMysticShotMissile", trueCoords.x, trueCoords.y)
	printChat("You used Q");
	
	-- This buff isnt in the main servers
    -- addBuff("Haste", 10.0, 1, getOwner(), getOwner())

end

function applyEffects()
    local attackDamage = getOwner():GetStats().AttackDamage
    local abilityPower = getOwner():GetStats().AbilityPower
    local damage = getEffectValue(0) + (1.1 * attackDamage.Total) + (0.4 * abilityPower.Total)
    dealPhysicalDamage(damage)
	
	
    -- TODO this can be fetched from projectile inibin "HitEffectName"
    addParticleTarget("Ezreal_mysticshot_tar.troy", getTarget())

    destroyProjectile()
end
