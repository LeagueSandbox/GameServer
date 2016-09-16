Vector2 = require 'Vector2' -- include 2d vector lib 

function onFinishCasting()
    local current = Vector2:new(owner.X, owner.Y)
    local to = (Vector2:new(spell.X, spell.Y) - current):normalize()
    local range = to * 445
    local trueCoords = current + range
    
    dashTo(owner, trueCoords.x, trueCoords.y, 1500, false, "SPELL3")
end
 
function applyEffects()
end
