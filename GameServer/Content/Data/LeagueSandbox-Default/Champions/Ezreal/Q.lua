Vector2 = require 'Vector2' -- include 2d vector lib

function onStartCasting()
    addParticleTarget(owner, "ezreal_bow.troy", owner, 1, "L_HAND")
end

function onFinishCasting()
    local current = Vector2:new(owner.X, owner.Y)
    local to = (Vector2:new(spell.X, spell.Y) - current):normalize()
    local range = to * 1150
    local trueCoords = current + range

    addProjectile("EzrealMysticShotMissile", trueCoords.x, trueCoords.y)
end

function applyEffects()
    local AD = owner:GetStats().AttackDamage.Total*1.1
    local AP = owner:GetStats().AbilityPower.Total*0.4
    local damage = getEffectValue(0) + AD + AP
    owner:dealDamageTo(getTarget(), damage, TYPE_PHYSICAL, SOURCE_ATTACK, false)
    lowerCooldown(0, 1)
    lowerCooldown(1, 1)
    lowerCooldown(2, 1)
    lowerCooldown(3, 1)
    destroyProjectile()
end

function onUpdate(diff)
end
