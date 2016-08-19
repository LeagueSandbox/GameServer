spell.FinishCasting:Add(function(sender,args)
    buff=me:AddBuff("MoveQuick", 5.0, 1, me)
	buff:SetMovementSpeedPercentModifier(12 + spell:getLevel() * 8)
end)