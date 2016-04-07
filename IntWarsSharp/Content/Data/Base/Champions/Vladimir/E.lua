function finishCasting()
	local speed = getEffectValue( 3 )
	local owner = getOwner()
	local myTeam = owner:getTeam()
	local units = getUnitsInRange( owner, 610, true )
	local haveBuff
	local currentStacks = 0

	for k, buff in pairs(getOwner():getBuffs()) do
		if buff ~= nil then
			if buff:getName("VladimirTidesofBloodCost") then
				haveBuff = true
				currentStacks = buff:getStacks()
			end
		end
	end
	for key,value in pairs( units ) do
		if myTeam ~= value:getTeam() and key ~= 1 then
			hitUnit = true
			owner:dealDamageTo( value, getEffectValue(0) + owner:getStats():getTotalAp()*getCoefficient() + currentStacks*getEffectValue(0)*0.25, DAMAGE_TYPE_TRUE, DAMAGE_SOURCE_SPELL );
			addParticleTarget("vladimir_base_e_tar.troy", value)
		end
	end
	if hitUnit then
		if haveBuff then
			if getOwner():getBuff("VladimirTidesofBloodCost"):getStacks() < 4 then
				getOwner():getBuff("VladimirTidesofBloodCost"):setStacks(getOwner():getBuff("VladimirTidesofBloodCost"):getStacks()+1)
				getOwner():getBuff("VladimirTidesofBloodCost"):setTimeElapsed(0)
			end
		else
			local buff = Buff.new("VladimirTidesofBloodCost", 10.0, BUFFTYPE_TEMPORARY, 1, getOwner())
			addBuff(buff, getOwner())
		end
		addParticleTarget("vladtidesofblood_bloodking_tar.troy", owner)
		addParticleTarget("briefheal.troy", owner)
	end
end

function applyEffects()
end
