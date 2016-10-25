Vector2 = require 'Vector2' -- include 2d vector lib

function onStartCasting()
    local current = Vector2:new(owner.X, owner.Y)
    local to = (Vector2:new(spell.X, spell.Y) - current):normalize()
    local range = to * 3340
    local trueCoords = current + range

    addLaser(trueCoords.x, trueCoords.y, true)

    addParticle(owner, "LuxMaliceCannon_beam.troy", trueCoords.x, trueCoords.y)
    spellAnimation("SPELL4", owner)
    addParticleTarget(owner, "LuxMaliceCannon_cas.troy", owner)
end

function onFinishCasting()

end

function applyEffects()
    dealMagicalDamage(200 + spell.Level * 100 + owner:GetStats().AbilityPower.Total * 0.75)
end

function onUpdate(diff)

end