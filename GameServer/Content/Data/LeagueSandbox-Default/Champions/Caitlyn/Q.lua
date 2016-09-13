Vector2 = require 'Vector2' -- include 2d vector lib 

function finishCasting()
    local current = Vector2:new(owner.X, owner.Y)
    local to = (Vector2:new(spell.X, spell.Y) - current):normalize()
    local range = to * 1150
    local trueCoords = current + range

    addServerProjectile(trueCoords.x, trueCoords.y)
end

function applyEffects()
    local reduc = math.min(getNumberObjectsHit(), 5)

    dealPhysicalDamage((getEffectValue(0) + owner:getStats():getTotalAd() + (1.3 * owner:getStats():getTotalAd())) + getEffectValue(0) * (1 - reduc/10))

    addParticleTarget("caitlyn_Base_peaceMaker_tar_02.troy", getTarget())
end
