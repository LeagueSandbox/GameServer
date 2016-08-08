function onAddBuff()
	printChat("onAddBuff")
end

function onUpdate(diff)
	printChat("onUpdate("..diff..")")
end

function onBuffEnd()
	printChat("onBuffEnd")
end