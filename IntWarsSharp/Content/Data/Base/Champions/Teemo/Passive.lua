function onUpdate(diff)
end

function onDamageTaken(attacker, damage, dmgType, source) --todo add callbacks for these 
   print("Damage taken is ".. damage)
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
