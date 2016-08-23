local timePassedTotal = 0

me.Update:Add(function(sender,diff)
    timePassedTotal = timePassedTotal+tonumber(diff)
	if(timePassedTotal >= 1000000) then
	   timePassedTotal = 0
	end
end)
