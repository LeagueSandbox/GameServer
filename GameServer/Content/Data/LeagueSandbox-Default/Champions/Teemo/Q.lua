Vector2 = require 'Vector2' -- include 2d vector lib 


spell.FinishCasting:Add(function(sender,args)
	local castTarget = spell:getTarget()
	local current = Vector2:new(me:getX(), me:getY())
	
	if current:distance(Vector2:new(castTarget:getX(), castTarget:getY())) <= 580 then	
		p=spell:addProjectileTarget("BlindingDart", castTarget, false)
		p.Hit:Add(function(sender,unit)
			if ( ( not ( castTarget == 0 ) ) and ( not isDead( castTarget ) ) ) then
				local baseDamage = 80
				local bonusDamage = me:GetStats().AbilityPower.Total*0.8
				local damage = baseDamage + bonusDamage
				me:dealDamageTo(unit, damage, DAMAGE_TYPE_MAGICAL, DAMAGE_SOURCE_SPELL );
				addBuff("Blind", 3, 1, unit, me)
				me:OnSpellEffect(unit)
			end
		end)
	end
end)




