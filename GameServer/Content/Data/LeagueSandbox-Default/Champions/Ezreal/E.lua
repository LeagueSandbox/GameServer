Vector2 = require 'Vector2' -- include 2d vector lib 

function finishCasting()
    local current = Vector2:new(getOwnerX(), getOwnerY())
    local to = Vector2:new(getSpellToX(), getSpellToY()) - current
    local trueCoords

    if to:length() > 475 then
        to = to:normalize()
        local range = to * 475
        trueCoords = current:copy() + range
    else
        trueCoords = Vector2:new(getSpellToX(), getSpellToY())
    end

    addParticle("Ezreal_arcaneshift_cas.troy", getOwnerX(), getOwnerY());
	trueCoords.x, trueCoords.y = getClosestTerrainExit(getOwner(), trueCoords.x, trueCoords.y, false)
    teleportTo(trueCoords.x, trueCoords.y)
    addParticleTarget("Ezreal_arcaneshift_flash.troy", getOwner());
	
	local target = nil
	local units = getUnitsInRange( getOwner(), 700, true )
	
	for key,value in pairs( units ) do
		local distance = 700
		if getOwner():getTeam() ~= value:getTeam() then
			if Vector2:new(trueCoords.x, trueCoords.y):distance(Vector2:new(value:getX(), value:getY())) <= distance then
				target = value
				distance = Vector2:new(trueCoords.x, trueCoords.y):distance(Vector2:new(value:getX(), value:getY()))
			end
		end
	end
	if target then
		addProjectileTarget(target)
	end
end

function applyEffects()
	dealMagicalDamage(getEffectValue(0)+getOwner():getStats():getTotalAp()*0.75)
	destroyProjectile()
end
