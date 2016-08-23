Vector2 = require 'Vector2' -- include 2d vector lib 

me.HitEffect:Add(function(source, unit)
	if(spell:getLevel() > 0) then
		local damage = tonumber(spell:getLevel()) * 10
		unit:dealDamageTo(unit, damage, DAMAGE_TYPE_MAGICAL, DAMAGE_SOURCE_SPELL);
        addParticleTarget("toxicshot_tar.troy", unit)
        addParticleTarget("Global_Poison.troy", unit)
		unit:AddBuff("ToxicShot", 4.0, 1, me)
	end
end)
