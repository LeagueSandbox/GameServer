Vector2 = require 'Vector2' -- include 2d vector lib 

function finishCasting()
    local current = Vector2:new(getOwnerX(), getOwnerY())
    local to = (Vector2:new(getSpellToX(), getSpellToY()) - current):normalize()
    local range = to * 975
    local trueCoords = current + range
    addProjectile(trueCoords.x, trueCoords.y)
end

function applyEffects()
    dealMagicalDamage(25+35*getSpellLevel()+getOwner():getStats():getTotalAp()*0.5)
    local buff = Buff.new("AhriSeduce", 0.75+getSpellLevel()*0.25, BUFFTYPE_TEMPORARY, getTarget(), getOwner())
    buff:setMovementSpeedPercentModifier(-50)
    addBuff(buff, getTarget())
    destroyProjectile()
end
