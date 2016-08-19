Vector2 = require 'Vector2' -- include 2d vector lib 

spell.FinishCasting:Add(function(sender,args)
	local current = Vector2:new(me:getX(), me:getY())
	local trueCoords = Vector2:new(spell:getX(), spell:getY())
	local to =  trueCoords - current 
 
    if to:length() > 230 then
        to = to:normalize()
        local range = to * 230
        trueCoords = current:copy() + range
    else
        trueCoords = Vector2:new(spell:getX(), spell:getY())
    end
    spell:AddPlaceable(trueCoords.x, trueCoords.y, "TeemoMushroom", "Noxious Trap")
end)