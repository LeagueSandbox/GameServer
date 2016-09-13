function finishCasting()
    if getChampionModel() ~= "Nidalee" then
        setChampionModel("Nidalee")
    else
        setChampionModel("Nidalee_Cougar")
    end	

    addParticleTarget("Nidalee_Base_R_Cas.troy", owner)
end

function applyEffects()
end
