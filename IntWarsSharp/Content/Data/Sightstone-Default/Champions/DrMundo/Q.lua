Vector2 = require 'Vector2' -- include 2d vector lib

function finishCasting()
    local current = Vector2:new(getOwnerX(), getOwnerY())
    local to = (Vector2:new(getSpellToX(), getSpellToY()) - current):normalize()
    local range = to * 1000
    local trueCoords = current + range

    addProjectile(trueCoords.x, trueCoords.y)
end

function applyEffects()
    dealPhysicalDamage(max(getEffectValue(0), getCastTarget():getStats():getCurrentHealth()))
	local buff = Buff.new("", 2.0, BUFFTYPE_TEMPORARY, getCastTarget(), getOwner())
    buff:setMovementSpeedPercentModifier(40)
    addBuff(buff, getCastTarget())
    destroyProjectile()
end
