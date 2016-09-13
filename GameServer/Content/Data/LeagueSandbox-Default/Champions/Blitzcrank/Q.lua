Vector2 = require 'Vector2' -- include 2d vector lib 

function finishCasting()
    local current = Vector2:new(owner.X, owner.Y)
    local to = (Vector2:new(spell.X, spell.Y) - current):normalize()
    local range = to * 925
    local trueCoords = current + range

    addProjectile(trueCoords.x, trueCoords.y)
end

function applyEffects()
    dealPhysicalDamage(getEffectValue(0)+owner:getStats():getTotalAp())

    local target = getTarget()
    local speed = projectileSpeed

    if ( not isDead( target ) ) then
		-- addParticleTarget("Blitzcrank_Grapplin_tar.troy", getTarget())

    	-- dash the user slightly in front of blitz
	    local current = Vector2:new(owner.X, owner.Y)
	    local to = (Vector2:new(spell.X, spell.Y) - current):normalize()
    	local range = to * 50
	    local trueCoords = current + range

    	dashTo( target, trueCoords.x, trueCoords.y, speed )
    end

    destroyProjectile()
end
