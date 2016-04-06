local timePassedTotal = 0

-- `diff` is a string containing the amount of micro seconds between two `onUpdate` calls
function onUpdate(diff)
	timePassedTotal = timePassedTotal+tonumber(diff)
	
	-- 1,000,000 microseconds == 1 second
	if(timePassedTotal >= 1000000) then
		print("Timepassed more than 10000")
		timePassedTotal = 0
		setMovementSpeed(0.0f)
	end
end
