function onFinishCasting()
	local units = getChampionsInRange(getOwner(), 850, true)
	local mostWoundedAlliedChampion = nil
	local lowestHealthPercentage = 100
	for i=0,units.Count-1 do
		value = units[i]
		if getOwner():getTeam() == value:getTeam() and i ~= 0 then
			local currentHealth = value:GetStats().CurrentHealth
			local maxHealth = value:GetStats().HealthPoints.Total
			if (currentHealth*100/maxHealth < lowestHealthPercentage) and (getOwner() ~= value) then
				lowestHealthPercentage = currentHealth*100/maxHealth
				mostWoundedAlliedChampion = value
			end
		end
	end
	
	if mostWoundedAlliedChampion then
		local newHealth = mostWoundedAlliedChampion:GetStats().CurrentHealth + 75 + getOwner():GetStats():GetLevel()*15
		local maxHealth = mostWoundedAlliedChampion:GetStats().HealthPoints.Total    
		if newHealth >= maxHealth then
			mostWoundedAlliedChampion:GetStats().CurrentHealth = maxHealth
		else
			mostWoundedAlliedChampion:GetStats().CurrentHealth = newHealth
		end
		
		local buff = Buff.new("Haste", 1.0, mostWoundedAlliedChampion, getOwner())
		buff:setMovementSpeedPercentModifier(30)
		addBuff(buff)
		
		addParticleTarget(mostWoundedAlliedChampion, "global_ss_heal_02.troy", mostWoundedAlliedChampion )
		addParticleTarget(mostWoundedAlliedChampion, "global_ss_heal_speedboost.troy", mostWoundedAlliedChampion )
	end
	newHealth = getOwner():GetStats().CurrentHealth + 75 + getOwner():GetStats():GetLevel()*15
	maxHealth = getOwner():GetStats().HealthPoints.Total
	if newHealth >= maxHealth then
		getOwner():GetStats().CurrentHealth = maxHealth
	else
		getOwner():GetStats().CurrentHealth = newHealth
	end
	
	local buff2 = Buff.new("Haste", 1.0, getOwner(), getOwner())
	buff2:setMovementSpeedPercentModifier(30)    
	addBuff(buff2)
	
	addParticleTarget(getOwner(), "global_ss_heal.troy", getOwner())	
	addParticleTarget(getOwner(), "global_ss_heal_speedboost.troy", getOwner())
end

function applyEffects()
end
