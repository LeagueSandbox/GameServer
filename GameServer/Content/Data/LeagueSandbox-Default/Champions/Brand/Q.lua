Vector2 = require 'Vector2' -- include 2d vector lib 

function finishCasting()
    local current = Vector2:new(getOwnerX(), getOwnerY())
    local to = (Vector2:new(getSpellToX(), getSpellToY()) - current):normalize()
    local range = to * 1050
    local trueCoords = current + range

    addProjectile(trueCoords.x, trueCoords.y)
end

function applyEffects()
	print(getEffectValue(0))
    dealMagicalDamage(getEffectValue(0)+getOwner():getStats():getTotalAp()*0.65)
    destroyProjectile()
end
