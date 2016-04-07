Vector2 = require 'Vector2' -- include 2d vector lib 

function finishCasting()
    local castTarget = getCastTarget()
    local current = Vector2:new(getOwnerX(), getOwnerY())
	local to = (Vector2:new(castTarget:getX(), castTarget:getY()) - current):normalize()
	local range = to * 625
	local trueCoords = current + range
    if current:distance(Vector2:new(castTarget:getX(), castTarget:getY())) <= 625 then
	dashTo(getOwner(), trueCoords.x, trueCoords.y, 625)
    else
        print("You're not even a baker")
		
    end
    
end

function applyEffects()
    local castTarget = getCastTarget()
	local current = Vector2:new(getOwnerX(), getOwnerY())
	if current:distance(Vector2:new(castTarget:getX(), castTarget:getY())) <= 625 then
	
	if ((not (castTarget == 0)) and (not isDead(castTarget))) then
		local owner = getOwner()
        local damage = getEffectValue(0) + owner:getStats():getTotalAd() 
        owner:dealDamageTo(castTarget, damage, DAMAGE_TYPE_PHYSICAL, DAMAGE_SOURCE_SPELL)  
    end
    else
        print("Stop trying to cast global bakerism!")
		
    end
    destroyProjectile()
end
