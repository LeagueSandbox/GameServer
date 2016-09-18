function onStartCasting()
end

function onFinishCasting()
	--addBuff("Recall", 8, owner, owner)
	addParticleTarget(owner, "TeleportHome.troy", owner)
end

function applyEffects()
end

function onUpdate(diff)
end
