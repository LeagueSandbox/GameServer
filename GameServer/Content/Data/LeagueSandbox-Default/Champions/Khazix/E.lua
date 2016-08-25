Vector2 = require 'Vector2' -- include 2d vector lib 

function onFinishCasting()
    local current = Vector2:new(getOwnerX(), getOwnerY())
    local to = (Vector2:new(getSpellToX(), getSpellToY()) - current):normalize()
    local range = to * 600
    local trueCoords = current + range
    
    dashTo(getOwner(), trueCoords.x, trueCoords.y, 809, 50, "SPELL3")
end
 
function applyEffects()
end
