Vector2 = require 'Vector2' -- include 2d vector lib

spell.FinishCasting:Add(function(sender,args)
    local current = Vector2:new(me:getX(), me:getY())
    local to = Vector2:new(spell:getX(), spell:getY()) - current
    local trueCoords
 
    if to:length() > 475 then
        to = to:normalize()
        local range = to * 475
        trueCoords = current:copy() + range
    else
        trueCoords = Vector2:new(spell:getX(), spell:getY())
    end
 
    addParticle(me, "Ezreal_arcaneshift_cas.troy", me:getX(), me:getY());
    teleportTo(me, trueCoords.x, trueCoords.y)
    addParticleTarget(me, "Ezreal_arcaneshift_flash.troy", me);
 
    local target = nil
    local units = getUnitsInRange(me, 700, true)
 
    for i=0,units.Count-1 do
		value = units[i]
        local distance = 700
        if me:getTeam() ~= value:getTeam() then
            if Vector2:new(trueCoords.x, trueCoords.y):distance(Vector2:new(value:getX(), value:getY())) <= distance then
                target = value
                distance = Vector2:new(trueCoords.x, trueCoords.y):distance(Vector2:new(value:getX(), value:getY()))
            end
        end
    end
    if target then
        p=spell:addProjectileTarget("EzrealArcaneShiftMissile", target)
		p.Hit:Add(function(sender,unit)
			me:dealDamageTo(unit, 25+spell:getLevel()*50+me:GetStats().AbilityPower.Total*0.75, DAMAGE_TYPE_MAGICAL, DAMAGE_SOURCE_SPELL )
			addParticleTarget(me, "Ezreal_arcaneshift_tar.troy", target)
			sender:setToRemove()
		end)
    end
end)


function removeEvents()
	
end