function finishCasting()
	local owner = getOwner()
	local myTeam = owner:getTeam()
	local units = getChampionsInRange( owner, 850, true )
	local lowestHealthPercentage = 100
	local mostWoundedAlliedChampion = nil
	for key,value in pairs( units ) do
		if myTeam == value:getTeam() and key ~= 1 then
			local currentHealth = value:getStats():getCurrentHealth()
			local maxHealth = value:getStats():getMaxHealth()
			if (currentHealth*100/maxHealth < lowestHealthPercentage) and (getOwner() ~= value) then
				lowestHealthPercentage = currentHealth*100/maxHealth
				mostWoundedAlliedChampion = value
			end
		end
	end
	
	if mostWoundedAlliedChampion then
		local newHealth = mostWoundedAlliedChampion:getStats():getCurrentHealth() + 75 + owner:getStats():getLevel()*15
		local maxHealth = mostWoundedAlliedChampion:getStats():getMaxHealth()    
		if newHealth >= maxHealth then
			mostWoundedAlliedChampion:getStats():setCurrentHealth(maxHealth)
		else
			mostWoundedAlliedChampion:getStats():setCurrentHealth(newHealth)
		end
		
		local buff = Buff.new("Haste", 1.0, BUFFTYPE_TEMPORARY, mostWoundedAlliedChampion, owner)
		buff:setMovementSpeedPercentModifier(30)
		addBuff(buff, mostWoundedAlliedChampion)
		
		addParticleTarget( "global_ss_heal_02.troy", mostWoundedAlliedChampion )
		addParticleTarget( "global_ss_heal_speedboost.troy", mostWoundedAlliedChampion )
	end
	newHealth = owner:getStats():getCurrentHealth() + 75 + owner:getStats():getLevel()*15
	maxHealth = owner:getStats():getMaxHealth()
	if newHealth >= maxHealth then
		owner:getStats():setCurrentHealth(maxHealth)
	else
		owner:getStats():setCurrentHealth(newHealth)
	end
	
	local buff2 = Buff.new("Haste", 1.0, BUFFTYPE_TEMPORARY, owner)    
	buff2:setMovementSpeedPercentModifier(30)    
	addBuff(buff2, owner)
	
	addParticleTarget( "global_ss_heal.troy", owner )	
	addParticleTarget( "global_ss_heal_speedboost.troy", owner )
	
end

function applyEffects()
end