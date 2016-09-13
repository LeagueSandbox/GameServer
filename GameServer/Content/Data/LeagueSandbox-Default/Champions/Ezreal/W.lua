Vector2 = require 'Vector2' -- include 2d vector lib 

function onFinishCasting()
    local current = Vector2:new(owner.X, owner.Y)
    local to = (Vector2:new(spell.X, spell.Y) - current):normalize()
    local range = to * 1000
    local trueCoords = current + range
	
    addProjectile("EzrealEssenceFluxMissile", trueCoords.x, trueCoords.y)
end

function applyEffects()
    if owner.Team ~= getTarget().Team then
        dealMagicalDamage(25+spellLevel*45+owner:GetStats().AbilityPower.Total*0.8)
	    addParticleTarget(owner, "Ezreal_essenceflux_tar.troy", getTarget())
    end
end
