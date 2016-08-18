Vector2 = require 'Vector2' -- include 2d vector lib 


function onFinishCasting()
    local current = Vector2:new(me:getX(), me:getY())
    local to = (Vector2:new(spell:getX(), spell:getY()) - current):normalize()
    local range = to * 20000
    local trueCoords = current + range

	p=spell:addProjectile("EzrealTrueshotBarrage", trueCoords.x, trueCoords.y)
	p.Hit:Add(function(sender,unit)
		local bonusAD = me:GetStats().AttackDamage.Total - me:GetStats().AttackDamage.BaseValue
		local AP = me:GetStats().AbilityPower.Total*0.9
		local damage = 200+spell:getLevel()*150 + bonusAD + AP
		me:dealDamageTo(unit, damage, DAMAGE_TYPE_MAGICAL, DAMAGE_SOURCE_SPELL)
		addParticleTarget(me, "Ezreal_TrueShot_tar.troy", unit)
		me:OnSpellEffect(unit)
		print(unit)
	end)
end

function removeEvents()
	
end


