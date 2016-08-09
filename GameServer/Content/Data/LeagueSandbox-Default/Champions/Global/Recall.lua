function onFinishCasting()
	addBuff("Recall", 8, getOwner(), getOwner())
	addParticleTarget(getOwner(), "TeleportHome.troy", getOwner())
end

function applyEffects()
end