Vector2 = require 'Vector2' -- include 2d vector lib 

function onFinishCasting()
    local current = Vector2:new(getOwnerX(), getOwnerY())
    local to = (Vector2:new(getSpellToX(), getSpellToY()) - current):normalize()
    local range = to * 99999
    local trueCoords = current + range

    addProjectile(trueCoords.x, trueCoords.y)	
end

function applyEffects()
    local reduc = math.min(getNumberObjectsHit(), 7)
    local damage = getEffectValue(0) + getOwner():GetStats().AttackDamage.FlatBonus + (getCoefficiant() * getOwner():GetStats():AbilityPower.Total)
	dealMagicalDamage(damage * (1 - reduc/10))
    -- TODO this can be fetched from projectile inibin "HitEffectName"
    addParticleTarget(getOwner(), "Ezreal_TrueShot_tar.troy", getTarget())
end
