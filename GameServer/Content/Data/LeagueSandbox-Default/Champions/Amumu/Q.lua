Vector2 = require 'Vector2' -- include 2d vector lib 

function finishCasting()
    local current = Vector2:new(getOwnerX(), getOwnerY())
    local to = (Vector2:new(getSpellToX(), getSpellToY()) - current):normalize()
    local range = to * 300
    local trueCoords = current + range

    addProjectileCustom("Bandage_beam.troy", 2000, trueCoords.x, trueCoords.y)
end

function applyEffects()
    dealPhysicalDamage(getEffectValue(0)+getOwner():getStats():getTotalAp())

    local target = getTarget()
    local speed = getProjectileSpeed()

    if ( not isDead( target ) ) then
		-- addParticleTarget("Blitzcrank_Grapplin_tar.troy", getTarget())

    	-- dash the user slightly in front of blitz
	    local current = Vector2:new(getOwnerX(), getOwnerY())
	    local to = (Vector2:new(getSpellToX(), getSpellToY()) - current):normalize()
    	local range = to * 50
	    local trueCoords = current + range

    	dashTo( target, trueCoords.x, trueCoords.y, speed )
    end

    destroyProjectile()
end
