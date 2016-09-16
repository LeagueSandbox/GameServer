Vector2 = require 'Vector2' -- include 2d vector lib 

function onFinishCasting()
    local current = Vector2:new(owner.X, owner.Y)
    local to = (Vector2:new(spell.X, spell.Y) - current):normalize()
    local range = to * 425
    local trueCoords = current + range
    
    dashTo(owner, trueCoords.x, trueCoords.y, 1200, false, "Spell3")
    addParticleTarget(owner, "Graves_Move_OnBuffActivate.troy", owner)
end

function applyEffects()
end
