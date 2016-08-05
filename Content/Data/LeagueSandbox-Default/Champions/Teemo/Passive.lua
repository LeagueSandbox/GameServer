-- variables of W passive
local timePassedTotal = 0
w_passive = 0


function onUpdate(diff)
   if w_passive == 1 then --If W passive is active
      -- Give him more speed
         local speedIncrease = 26
         local buff = Buff.new("MoveQuick", 1.0, BUFFTYPE_TEMPORARY, getOwner())
         buff:setMovementSpeedPercentModifier(speedIncrease)
         addBuff(buff, getOwner())
   end
   if w_passive == 0 then --If W passive is not  active
      timePassedTotal = timePassedTotal+tonumber(diff)
   	-- 1,000,000 microseconds == 1 second
	   if(timePassedTotal >= 5000000) then
		   timePassedTotal = 0
		   w_passive = 1
	   end
   end
end

function onDamageTaken(attacker, damage, dmgType, source) --todo add callbacks for these 
   print("Damage taken is ".. damage)
   w_passive = 0 --on take damage it disables
end

function onAutoAttack(target)
    print("We just auto attacked!")
    
    local bonus = tonumber(me:getSpell(2):getLevel()) * 10
    
    if(bonus ~= 0) then
        --me:dealDamageTo(target,bonus,damageType,source)
        
        local buff = Buff.new("ToxicShot", 4.0, BUFFTYPE_TEMPORARY, target, me)
        
        dealMagicDamage(target, bonus) --temporarily using this method until we get enums working in Lua
        addBuff(buff, target)
        
        print("We are dealing bonus damage " .. bonus)
        
        addParticleTarget("toxicshot_tar.troy", target)
        addParticleTarget("Global_Poison.troy", target)
    end
end

function onDealDamage(target, damage, damageType, source)
end
