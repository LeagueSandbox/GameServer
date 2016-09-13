Vector2 = require 'Vector2' -- include 2d vector lib 

function finishCasting()
	local castTarget = castTarget
	local current = Vector2:new(owner.X, owner.Y)
	if current:distance(Vector2:new(castTarget.X, castTarget.Y)) <= 625 then
		addParticleTarget("Kassadin_Base_Q_ShieldOn.troy", owner)
		addParticleTarget("Kassadin_Base_Q_Shield.troy", owner)	
		addProjectileTarget( castTarget )
	else
		print("Target is too far away")
	end
	
end

function applyEffects()
	local castTarget = castTarget

    if ( ( not ( castTarget == 0 ) ) and ( not isDead( castTarget ) ) ) then
		print(getEffectValue(0))
		local owner = owner;
		local damage = getEffectValue(0) + owner:getStats():getTotalAp()*0.7

		owner:dealDamageTo( castTarget, damage, DAMAGE_TYPE_MAGICAL, DAMAGE_SOURCE_SPELL );
		addParticleTarget("Kassadin_Base_Q_tar.troy", getTarget())
	end
    destroyProjectile()
    
end
