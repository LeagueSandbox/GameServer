Vector2 = require 'Vector2' -- include 2d vector lib

function onStartCasting()
end

function onFinishCasting()
    local current = Vector2:new(owner.X, owner.Y)
    local to = Vector2:new(spell.X, spell.Y) - current
    local trueCoords
	
    if to:length() > 425 then
        to = to:normalize()
        local range = to * 425
        trueCoords = current:copy() + range
    else
        trueCoords = Vector2:new(spell.X, spell.Y)
    end
	addParticle(owner, "global_ss_flash.troy", owner.X, owner.Y)
    teleportTo(owner, trueCoords.x, trueCoords.y)
	addParticleTarget(owner, "global_ss_flash_02.troy", owner)
end

function applyEffects()
end

function onUpdate(diff)
end
