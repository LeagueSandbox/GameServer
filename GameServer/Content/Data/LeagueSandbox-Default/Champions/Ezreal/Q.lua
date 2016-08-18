Vector2 = require 'Vector2' -- include 2d vector lib 

spell.FinishCasting:Add(function(sender,args)
	local current = Vector2:new(me:getX(), me:getY())
    local to = (Vector2:new(spell:getX(), spell:getY()) - current):normalize()
    local range = to * 1150
    local trueCoords = current + range
	p=spell:addProjectile("EzrealMysticShotMissile", trueCoords.x, trueCoords.y)
	p.Hit:Add(function(sender,unit)
		local AD = me:GetStats().AttackDamage.Total*1.1
		local AP = me:GetStats().AbilityPower.Total*0.4
		local damage = spell:getEffectValue(0) + AD + AP
		me:dealDamageTo(unit, damage, DAMAGE_TYPE_PHYSICAL, DAMAGE_SOURCE_SPELL)
		addParticleTarget(me, "Ezreal_mysticshot_tar.troy", unit)
		me:onHit(unit)
		p:setToRemove()
	end)
end)

function removeEvents()
	
end