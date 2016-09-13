
Vector2 = require 'Vector2' -- include 2d vector lib 

function finishCasting()
    local current = Vector2:new(owner.X, owner.Y)
    local to = (Vector2:new(spell.X, spell.Y) - current):normalize()
    local range = to * 1000
    local trueCoords = current + range

    addProjectile(trueCoords.x, trueCoords.y)
end

function applyEffects()
    dealPhysicalDamage(getEffectValue(1.85)+owner:getStats():getTotalAd()+(0*owner:getStats():getTotalAp()))
    -- TODO this can be fetched from projectile inibin "HitEffectName"
    addParticleTarget("UrgotLineMissile_mis.troybin", getTarget())

    destroyProjectile()
end