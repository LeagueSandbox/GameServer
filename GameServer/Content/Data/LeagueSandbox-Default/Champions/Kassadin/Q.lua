Vector2 = require 'Vector2' -- include 2d vector lib 

function finishCasting()
	local castTarget = getCastTarget()
	local current = Vector2:new(getOwnerX(), getOwnerY())
	if current:distance(Vector2:new(castTarget:getX(), castTarget:getY())) <= 625 then
		addParticleTarget("Kassadin_Base_Q_ShieldOn.troy", getOwner())
		addParticleTarget("Kassadin_Base_Q_Shield.troy", getOwner())	
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
		local damage = getEffectValue(0) + owner:getStats():getTotalAp()*0.7

		owner:dealDamageTo( castTarget, damage, DAMAGE_TYPE_MAGICAL, DAMAGE_SOURCE_SPELL );
		addParticleTarget("Kassadin_Base_Q_tar.troy", getTarget())
	end
    destroyProjectile()
    
end
