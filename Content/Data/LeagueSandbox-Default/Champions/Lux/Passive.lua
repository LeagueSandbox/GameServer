function onUpdate(diff)
    --print("Onupdate")
end

function onDamageTaken(attacker, damage, dmgType, source)
    print("Damage taken is ".. damage)
end

function onAutoAttack(us, target)
    print("We just auto attacked!")
end
