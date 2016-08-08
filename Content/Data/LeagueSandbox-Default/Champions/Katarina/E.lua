Vector2 = require 'Vector2' -- include 2d vector lib 

function finishCasting()
	local castTarget = getCastTarget()
	local current = Vector2:new(getOwnerX(), getOwnerY())
	if current:distance(Vector2:new(castTarget:getX(), castTarget:getY())) <= 700 then
		teleportTo(castTarget:getX(), castTarget:getY())
		if ( ( not ( castTarget == 0 ) ) and ( not isDead( castTarget ) ) ) then
			print(getEffectValue(0))
			local owner = getOwner();
			local damage = getEffectValue(0) + owner:getStats():getTotalAp()*0.25

			owner:dealDamageTo( castTarget, damage, DAMAGE_TYPE_MAGICAL, DAMAGE_SOURCE_SPELL );
		end
	else
		print("Target is too far away")
	end
end

function applyEffects()
end
