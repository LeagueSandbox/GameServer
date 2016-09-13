Vector2 = require 'Vector2' -- include 2d vector lib 

function onFinishCasting()
    local current = Vector2:new(owner.X, owner.Y)
    local to = Vector2:new(spell.X, spell.Y) - current
    local trueCoords
 
    if to:length() > 230 then
        to = to:normalize()
        local range = to * 230
        trueCoords = current:copy() + range
    else
        trueCoords = Vector2:new(spell.X, spell.Y)
    end
  
    addPlaceable(trueCoords.x, trueCoords.y, "TeemoMushroom", "Noxious Trap")
end

function applyEffects()
end