Vector2 = require 'Vector2' -- include 2d vector lib 

function finishCasting()
    local current = Vector2:new(owner.X, owner.Y)
    local to = (Vector2:new(spell.X, spell.Y) - current):normalize()
    local range = to * 1500
    local trueCoords = current + range

    addProjectile(trueCoords.x, trueCoords.y)
end

function applyEffects()
	print(getEffectValue(0))
	print(getEffectValue(1))
    dealPhysicalDamage(getEffectValue(0)+owner:getStats():getTotalAd()*coefficient)
	local buff = Buff.new("Slow", 2.0, BUFFTYPE_TEMPORARY, getTarget(), owner)
    buff:setMovementSpeedPercentModifier(getEffectValue(1))
    addBuff(buff, getTarget())
    destroyProjectile()
end
