Vector2 = require 'Vector2' -- include 2d vector lib 

function finishCasting()
    local current = Vector2:new(owner.X, owner.Y)
    local to = (Vector2:new(spell.X, spell.Y) - current):normalize()
    local range = to * 975
    local trueCoords = current + range
    addProjectile(trueCoords.x, trueCoords.y)
end

function applyEffects()
    dealMagicalDamage(25+35*spellLevel+owner:getStats():getTotalAp()*0.5)
    local buff = Buff.new("AhriSeduce", 0.75+spellLevel*0.25, BUFFTYPE_TEMPORARY, getTarget(), owner)
    buff:setMovementSpeedPercentModifier(-50)
    addBuff(buff, getTarget())
    destroyProjectile()
end
