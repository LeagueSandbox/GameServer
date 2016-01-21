Vector2 = require 'Vector2' -- include 2d vector lib 

function finishCasting()
	local castTarget = getCastTarget()
    addProjectileTargetCustom( "pirate_parley_tar.troy", 0, castTarget )
end

function applyEffects()
	local castTarget = getCastTarget()

    if ( ( not ( castTarget == 0 ) ) and ( not isDead( castTarget ) ) ) then
		local owner = getOwner();
		local damage = getEffectValue(0) + owner:getStats():getTotalAd()

		owner:dealDamageTo( castTarget, damage, DAMAGE_TYPE_PHYSICAL, DAMAGE_SOURCE_SPELL );
	end

    destroyProjectile()
end
