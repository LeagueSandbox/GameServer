Vector2 = require 'Vector2' -- include 2d vector lib

function onStartCasting()
    addParticleTarget(owner, "Ezreal_bow_huge.troy", owner, 1, "L_HAND")
end

function onFinishCasting()
    local current = Vector2:new(owner.X, owner.Y)
    local to = (Vector2:new(spell.X, spell.Y) - current):normalize()
    local range = to * 20000
    local trueCoords = current + range

    addProjectile("EzrealTrueshotBarrage", trueCoords.x, trueCoords.y, true)	
end

function applyEffects()
    local reduc = math.min(getNumberObjectsHit(), 7)
    local bonusAD = owner:GetStats().AttackDamage.Total - owner:GetStats().AttackDamage.BaseValue
    local AP = owner:GetStats().AbilityPower.Total*0.9
    local damage = 200+spellLevel*150 + bonusAD + AP
	dealMagicalDamage(damage * (1 - reduc/10))
end

function onUpdate(diff)
end
