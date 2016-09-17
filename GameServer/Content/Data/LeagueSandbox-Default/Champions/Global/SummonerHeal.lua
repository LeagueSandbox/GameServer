function onStartCasting()
end

function onFinishCasting()
	local units = getChampionsInRange(owner, 850, true)
	local mostWoundedAlliedChampion = nil
	local lowestHealthPercentage = 100
	for i=0,units.Count-1 do
		value = units[i]
		if owner.Team == value.Team and i ~= 0 then
			local currentHealth = value:GetStats().CurrentHealth
			local maxHealth = value:GetStats().HealthPoints.Total
			if (currentHealth*100/maxHealth < lowestHealthPercentage) and (owner ~= value) then
				lowestHealthPercentage = currentHealth*100/maxHealth
				mostWoundedAlliedChampion = value
			end
		end
	end
	
	if mostWoundedAlliedChampion then
		local newHealth = mostWoundedAlliedChampion:GetStats().CurrentHealth + 75 + owner:GetStats():GetLevel()*15
		local maxHealth = mostWoundedAlliedChampion:GetStats().HealthPoints.Total    
		if newHealth >= maxHealth then
			mostWoundedAlliedChampion:GetStats().CurrentHealth = maxHealth
		else
			mostWoundedAlliedChampion:GetStats().CurrentHealth = newHealth
		end
		
		--local buff = Buff.new("Haste", 1.0, mostWoundedAlliedChampion, owner)
		--buff:setMovementSpeedPercentModifier(30)
		--addBuff(buff)
		
		addParticleTarget(mostWoundedAlliedChampion, "global_ss_heal_02.troy", mostWoundedAlliedChampion )
		addParticleTarget(mostWoundedAlliedChampion, "global_ss_heal_speedboost.troy", mostWoundedAlliedChampion )
	end
	newHealth = owner:GetStats().CurrentHealth + 75 + owner:GetStats():GetLevel()*15
	maxHealth = owner:GetStats().HealthPoints.Total
	if newHealth >= maxHealth then
		owner:GetStats().CurrentHealth = maxHealth
	else
		owner:GetStats().CurrentHealth = newHealth
	end
	
	--local buff2 = Buff.new("Haste", 1.0, owner, owner)
	--buff2:setMovementSpeedPercentModifier(30)    
	--addBuff(buff2)
	
	addParticleTarget(owner, "global_ss_heal.troy", owner)	
	addParticleTarget(owner, "global_ss_heal_speedboost.troy", owner)
end

function applyEffects()
end

function onUpdate(diff)
end
