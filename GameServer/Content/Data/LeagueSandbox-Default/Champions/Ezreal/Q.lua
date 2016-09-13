Vector2 = require 'Vector2' -- include 2d vector lib 

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
    dealPhysicalDamage(damage)
	
	-- TODO this can be fetched from projectile inibin "HitEffectName"
    addParticleTarget(owner, "Ezreal_mysticshot_tar.troy", getTarget())

    destroyProjectile()
end
