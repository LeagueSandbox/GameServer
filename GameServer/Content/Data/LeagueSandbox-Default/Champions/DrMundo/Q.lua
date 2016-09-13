Vector2 = require 'Vector2' -- include 2d vector lib

function finishCasting()
    local current = Vector2:new(owner.X, owner.Y)
    local to = (Vector2:new(spell.X, spell.Y) - current):normalize()
    local range = to * 1000
    local trueCoords = current + range

    addProjectile(trueCoords.x, trueCoords.y)
end

function applyEffects()
    dealPhysicalDamage(max(getEffectValue(0), castTarget:getStats():getCurrentHealth()))
	local buff = Buff.new("", 2.0, BUFFTYPE_TEMPORARY, castTarget, owner)
    buff:setMovementSpeedPercentModifier(40)
    addBuff(buff, castTarget)
    destroyProjectile()
end
