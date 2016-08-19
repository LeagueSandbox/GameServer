local timePassedTotal = 0

buff.Update:Add(function(sender,diff)
    timePassedTotal = timePassedTotal+tonumber(diff)
	if(timePassedTotal >= 1000000) then
		timePassedTotal = 0
		source:dealDamageTo(me, 10, DAMAGE_TYPE_MAGICAL, DAMAGE_SOURCE_SPELL);
	end
end)