Vector2 = require 'Vector2' -- include 2d vector lib 

function onFinishCasting()
    local current = Vector2:new(getOwnerX(), getOwnerY())
    local to = (Vector2:new(getSpellToX(), getSpellToY()) - current):normalize()
    local range = to * 445
    local trueCoords = current + range
    
    dashTo(getOwner(), trueCoords.x, trueCoords.y, 1500, 0, "SPELL3")
end
 
function applyEffects()
end
