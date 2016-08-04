Vector2 = require 'Vector2' -- include 2d vector lib 

function onFinishCasting()
    local current = Vector2:new(getOwnerX(), getOwnerY())
    local to = (Vector2:new(getSpellToX(), getSpellToY()) - current):normalize()
    local range = to * 1000
    local trueCoords = current + range
	
    addProjectile("EzrealEssenceFluxMissile", trueCoords.x, trueCoords.y)
end

function applyEffects()
    if getOwner():getTeam() ~= getTarget():getTeam() then
        dealMagicalDamage(25+getSpellLevel()*45+getOwner():GetStats().AbilityPower.Total*0.8)
	    addParticleTarget(getOwner(), "Ezreal_essenceflux_tar.troy", getTarget())
    end
end
