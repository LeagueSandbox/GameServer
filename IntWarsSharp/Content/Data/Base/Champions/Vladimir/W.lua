Vector2 = require 'Vector2' -- include 2d vector lib 

function finishCasting()
	addParticleTarget("vladimir_base_w_buf.troy", getOwner())
	local buff = Buff.new("VladimirSanguinePool", 2.0, BUFFTYPE_TEMPORARY, 1, getOwner())
	buff:resetAnimationsWhenRemoved(true)
	addBuff(buff, getOwner())
	setAnimation("RUN", "SPELL2DOWN", getOwner())
	--resetAnimations(getOwner())
end

function applyEffects()
end
