Vector2 = require 'Vector2' -- include 2d vector lib 

function finishCasting()
    local current = Vector2:new(getOwnerX(), getOwnerY())
    local to = Vector2:new(getSpellToX(), getSpellToY()) - current
    local trueCoords

    if(to:length() > 400) then -- getRange() returns 200
        to = to:normalize()
        local range = to * 400 -- getRange() returns 200
        trueCoords = Vector2:new(current.x, current.y) + range
    else
        trueCoords = Vector2:new(getSpellToX(), getSpellToY())
    end

    teleportTo(trueCoords.x, trueCoords.y)
end

function applyEffects()
end
