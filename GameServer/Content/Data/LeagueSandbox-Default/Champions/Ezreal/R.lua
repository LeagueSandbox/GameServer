Vector2 = require 'Vector2' -- include 2d vector lib 

function onFinishCasting()
    local current = Vector2:new(owner.X, owner.Y)
    local to = (Vector2:new(spell.X, spell.Y) - current):normalize()
    local range = to * 20000
    local trueCoords = current + range

    addProjectile("EzrealTrueshotBarrage", trueCoords.x, trueCoords.y)	
end

function applyEffects()
    local reduc = math.min(getNumberObjectsHit(), 7)
    local bonusAD = owner:GetStats().AttackDamage.Total - owner:GetStats().AttackDamage.BaseValue
    local AP = owner:GetStats().AbilityPower.Total*0.9
    local damage = 200+spellLevel*150 + bonusAD + AP
	dealMagicalDamage(damage * (1 - reduc/10))
    
    addParticleTarget(owner, "Ezreal_TrueShot_tar.troy", getTarget())
end
