Vector2 = require 'Vector2' -- include 2d vector lib 

function finishCasting()
	spellAnimation("Spell4", getOwner())
    local current = Vector2:new(getOwnerX(), getOwnerY())
    local to = (Vector2:new(getSpellToX(), getSpellToY()) - current):normalize()
    local range = to * 450
    local trueCoords = current + range    
    dashTo(getOwner(), trueCoords.x, trueCoords.y, 1200)
end

function applyEffects()
end