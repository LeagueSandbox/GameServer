Vector2 = require 'Vector2' -- include 2d vector lib

function onStartCasting()
end

function onFinishCasting()
    addProjectileTarget("AkaliMota", castTarget, true)
end

function applyEffects()
    local damage = 15 + spellLevel * 20 + owner:GetStats().AbilityPower.Total * 0.4
    dealMagicalDamage(damage)
    destroyProjectile()
end

function onUpdate(diff)
end
