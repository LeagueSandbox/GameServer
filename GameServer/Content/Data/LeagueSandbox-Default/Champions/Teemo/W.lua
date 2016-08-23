local timePassedTotal = 0
local buff = nil

me.RecieveDamage:Add(function(sender,args)
	timePassedTotal = 0
	if buf ~= nil then
		if buff:GetDuration() == 0 then
			me:RemoveBuff(buff)
			buff = nil
		end
	end
end)

spell.FinishCasting:Add(function(sender,args)
    buff=me:AddBuff("MoveQuick", 5.0, 1, me)
	buff:SetMovementSpeedPercentModifier(12 + spell:getLevel() * 8)
end)

spell.Update:Add(function(sender,diff)
    timePassedTotal = timePassedTotal+tonumber(diff)
	if(timePassedTotal >= 5000000) then
		timePassedTotal = 0
		if buff == nil then
			buff=me:AddBuff("MoveQuick", 0.0, 1, me)
			buff:SetMovementSpeedPercentModifier(12 + spell:getLevel() * 8)
		end
	end
end)