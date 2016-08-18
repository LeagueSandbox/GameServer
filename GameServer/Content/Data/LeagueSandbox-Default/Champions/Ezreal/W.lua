Vector2 = require 'Vector2' -- include 2d vector lib 

spell.FinishCasting:Add(function(sender,args)
	local current = Vector2:new(me:getX(), me:getY())
    local to = (Vector2:new(spell:getX(), spell:getY()) - current):normalize()
    local range = to * 1000
    local trueCoords = current + range
	p=spell:addProjectile("EzrealEssenceFluxMissile", trueCoords.x, trueCoords.y)
	
	p.Hit:Add(function(sender,unit)
		if me:getTeam() ~= unit:getTeam() then
			me:dealDamageTo(unit, 25+getSpellLevel()*45+me:GetStats().AbilityPower.Total*0.8, DAMAGE_TYPE_MAGICAL, DAMAGE_SOURCE_SPELL)
			addParticleTarget(me, "Ezreal_essenceflux_tar.troy", unit)
			me:OnSpellEffect(unit)
		end
	end)
end)

function removeEvents()
	
end

