Vector2 = require 'Vector2' -- include 2d vector lib 

function finishCasting()
    local current = Vector2:new(getOwnerX(), getOwnerY())
    local to = (Vector2:new(getSpellToX(), getSpellToY()) - current):normalize()
    local range = to * 99999
    local trueCoords = current + range

    addProjectile(trueCoords.x, trueCoords.y)	
end

function applyEffects()
    local reduc = math.min(getNumberObjectsHit(), 5)
    local damage = getEffectValue(0) + getOwner():getStats():getBonusAdFlat() + (0.9 * getOwner():getStats():getTotalAp())
	dealMagicalDamage(damage * (1 - reduc/10))
    -- TODO this can be fetched from projectile inibin "HitEffectName"
    addParticleTarget("Ezreal_TrueShot_tar.troy", getTarget())
end
