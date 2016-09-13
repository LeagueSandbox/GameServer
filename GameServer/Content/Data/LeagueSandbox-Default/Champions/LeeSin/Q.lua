Vector2 = require 'Vector2' -- include 2d vector lib 

function finishCasting()
    local current = Vector2:new(owner.X, owner.Y)
    local to = (Vector2:new(spell.X, spell.Y) - current):normalize()
    local range = to * 1100
    local trueCoords = current + range
	--addProjectile("BlindMonkQOne", trueCoords.x, trueCoords.y)
end

function applyEffects()
    addBuff("BlindMonkSonicWave", 5.0, 1, getTarget(), owner)
    addParticleTarget(owner, "BlindMonkQOne.troy", getTarget())
    destroyProjectile()
end