local timePassedTotal = 0

function finishCasting()
	addBuff("Recall", 8, BUFFTYPE_TEMPORARY, getOwner())
	addParticleTarget( "TeleportHome.troy", getOwner() )
end

function applyEffects()
	
end