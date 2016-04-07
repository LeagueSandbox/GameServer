Vector2 = require 'Vector2' -- include 2d vector lib

function finishCasting()
	local castTarget = getCastTarget()
	local current = Vector2:new(getOwnerX(), getOwnerY())
	if current:distance(Vector2:new(castTarget:getX(), castTarget:getY())) <= 600 then
		addProjectileTarget( castTarget )
	else
		print("Target is too far away")
	end

end

function applyEffects()
	print(getEffectValue(0))
	local castTarget = getCastTarget()

    if ( ( not ( castTarget == 0 ) ) and ( not isDead( castTarget ) ) ) then
		local owner = getOwner();
		local damage = getEffectValue(1) + owner:getStats():getTotalAd()*1.4

		owner:dealDamageTo( castTarget, damage, DAMAGE_TYPE_PHYSICAL, DAMAGE_SOURCE_SPELL );
	end

    destroyProjectile()
end