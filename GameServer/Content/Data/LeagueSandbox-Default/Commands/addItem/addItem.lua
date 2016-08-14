registerCommand("additem", "additem idPlayer idItem")

function onExecute()
    if args.Length < 3 then
        printChat("Not enough args")
        return
    end
    addItem(getChampion(tonumber(args[1])), tonumber(args[2]))
end