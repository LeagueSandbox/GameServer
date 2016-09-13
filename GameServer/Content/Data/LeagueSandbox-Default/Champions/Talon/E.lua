Vector2 = require 'Vector2' -- include 2d vector lib 

function finishCasting()
	local castTarget = castTarget
	local current = Vector2:new(owner.X, owner.Y)
	if current:distance(Vector2:new(castTarget.X, castTarget.Y)) <= 700 then
		teleportTo(castTarget.X, castTarget.Y)
		if ( ( not ( castTarget == 0 ) ) and ( not isDead( castTarget ) ) ) then
			print(getEffectValue(0))
			local owner = owner;
			local buff = Buff.new("", 0.25, BUFFTYPE_TEMPORARY, owner)
			buff:setMovementSpeedPercentModifier(-99)
			addBuff(buff, castTarget)
		end
	else
		print("Target is too far away")
	end
end

function applyEffects()
end
