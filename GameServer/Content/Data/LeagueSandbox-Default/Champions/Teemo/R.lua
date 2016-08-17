Vector2 = require 'Vector2' -- include 2d vector lib 

function onFinishCasting()
    local current = Vector2:new(getOwnerX(), getOwnerY())
    local to = Vector2:new(getSpellToX(), getSpellToY()) - current
    local trueCoords
 
    if to:length() > 230 then
        to = to:normalize()
        local range = to * 230
        trueCoords = current:copy() + range
    else
        trueCoords = Vector2:new(getSpellToX(), getSpellToY())
    end
  
    addPlaceable(trueCoords.x, trueCoords.y, "TeemoMushroom", "Noxious Trap")
end

function applyEffects()
end