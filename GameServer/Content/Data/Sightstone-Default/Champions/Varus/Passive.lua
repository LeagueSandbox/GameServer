function onUpdate(diff)
end

function onDamageTaken(attacker, damage, dmgType, source)
   print("Damage taken is ".. damage)
end

function onAutoAttack(target)
    print("We just auto attacked!")
    
    local bonus = 6 + tonumber(me:getSpell(1):getLevel()) * 4 + me:getStats():getTotalAp()*0.25
    if(me:getSpell(1):getLevel() > 0) then
		print("Bonus damage")
        dealMagicDamage(target, bonus)
    end
end

function onDealDamage(target, damage, damageType, source)
end
