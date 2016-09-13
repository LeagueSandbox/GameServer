Vector2 = require 'Vector2' -- include 2d vector lib 

function finishCasting()
	addParticleTarget("vladimir_base_w_buf.troy", owner)
	local buff = Buff.new("VladimirSanguinePool", 2.0, BUFFTYPE_TEMPORARY, 1, owner)
	buff:resetAnimationsWhenRemoved(true)
	addBuff(buff, owner)
	setAnimation("RUN", "SPELL2DOWN", owner)
	--resetAnimations(owner)
end

function applyEffects()
end
