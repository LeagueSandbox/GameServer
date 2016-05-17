Vector2 = require 'Vector2' -- include 2d vector lib 

function onFinishCasting()
    local current = Vector2:new(getOwnerX(), getOwnerY())
    local to = (Vector2:new(getSpellToX(), getSpellToY()) - current):normalize()
    local range = to * 1150
    local trueCoords = current + range

    addProjectile("EzrealMysticShotMissile", trueCoords.x, trueCoords.y)
end

function applyEffects()
    local attackDamage = getOwner():GetStats().AttackDamage
    local abilityPower = getOwner():GetStats().AbilityPower
    local damage = getEffectValue(0) + attackDamage.Total + (0.4 * abilityPower.Total)
    dealPhysicalDamage(damage)
    -- TODO this can be fetched from projectile inibin "HitEffectName"
    addParticleTarget(getOwner(), "Ezreal_mysticshot_tar.troy", getTarget())

    destroyProjectile()
end
