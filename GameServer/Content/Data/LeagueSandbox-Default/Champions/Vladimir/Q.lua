Vector2 = require 'Vector2' -- include 2d vector lib 

function finishCasting()
	local castTarget = castTarget
	local current = Vector2:new(owner.X, owner.Y)
	if current:distance(Vector2:new(castTarget.X, castTarget.Y)) <= 600 then	
		print(getEffectValue(0))
		print(getEffectValue(1))
		local owner = owner;
		owner:dealDamageTo( castTarget, getEffectValue(0) + owner:getStats():getTotalAp()*0.6, DAMAGE_TYPE_TRUE, DAMAGE_SOURCE_SPELL );		
		
		local newHealth = owner:getStats():getCurrentHealth() + getEffectValue(1) + owner:getStats():getTotalAp()*0.25
    	local maxHealth = owner:getStats():getMaxHealth()    
    	if newHealth >= maxHealth then
			owner:getStats():setCurrentHealth(maxHealth)
    	else
			owner:getStats():setCurrentHealth(newHealth)
    	end    
	else
		print("Target is too far away")
	end
	
end

function applyEffects()
end
