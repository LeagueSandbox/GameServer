Vector2 = require 'Vector2' -- include 2d vector lib 

function finishCasting()
	spellAnimation("Spell4", owner)
    local current = Vector2:new(owner.X, owner.Y)
    local to = (Vector2:new(spell.X, spell.Y) - current):normalize()
    local range = to * 450
    local trueCoords = current + range    
    dashTo(owner, trueCoords.x, trueCoords.y, 1200)
end

function applyEffects()
end