Vector2 = require 'Vector2' -- include 2d vector lib 

function finishCasting()
	local castTarget = getCastTarget()
	local current = Vector2:new(getOwnerX(), getOwnerY())
	if current:distance(Vector2:new(castTarget:getX(), castTarget:getY())) <= 625 then	
		addProjectileTarget( castTarget )
	else
		print("Target is too far away")
	end
	
end

function applyEffects()
	local castTarget = getCastTarget()

    if ( ( not ( castTarget == 0 ) ) and ( not isDead( castTarget ) ) ) then
		print(getEffectValue(0))
		local owner = getOwner();
		local damage = getEffectValue(0) + owner:getStats():getTotalAp()*0.8

		owner:dealDamageTo( castTarget, damage, DAMAGE_TYPE_MAGICAL, DAMAGE_SOURCE_SPELL );
	end

    destroyProjectile()
end
