function onUpdate(diff)
    --print("Onupdate")
end

function onDamageTaken(attacker, damage, dmgType, source)
    printChat("Damage taken is ".. damage)
end

function onAutoAttack(us, target)
end

function onDealDamage(target, damage, damageType, source)
end
