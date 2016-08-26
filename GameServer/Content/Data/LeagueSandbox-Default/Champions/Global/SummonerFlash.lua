Vector2 = require 'Vector2' -- include 2d vector lib 

function onFinishCasting()
    local current = Vector2:new(getOwnerX(), getOwnerY())
    local to = Vector2:new(getSpellToX(), getSpellToY()) - current
    local trueCoords
	
    if to:length() > 425 then
        to = to:normalize()
        local range = to * 425
        trueCoords = current:copy() + range
    else
        trueCoords = Vector2:new(getSpellToX(), getSpellToY())
    end
	addParticle(getOwner(), "global_ss_flash.troy", getOwnerX(), getOwnerY())
    teleportTo(getOwner(), trueCoords.x, trueCoords.y)
	addParticleTarget(getOwner(), "global_ss_flash_02.troy", getOwner())
end

function applyEffects()
end