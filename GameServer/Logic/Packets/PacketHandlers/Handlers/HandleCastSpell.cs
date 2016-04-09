using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using static ENet.Native;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.GameObjects;
using System.Numerics;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleCastSpell : IPacketHandler
    {
        public unsafe bool HandlePacket(ENetPeer* peer, byte[] data, Game game)
        {
            var spell = new CastSpell(data);
            //Todo spellslot 0-3 qwer, 4-5 d f, 6-11 items

            // There are some bits triggering this
            /*if ((spell.spellSlotType & 0x0F) > 0)
            {
                Logger.LogCoreInfo("Summoner Spell Cast");
                Logger.LogCoreInfo("Type: " + spell.spellSlotType.ToString("x") + ", Slot " + spell.spellSlot + ", coord " + spell.x + " ; " + spell.y + ", coord2 " + spell.x2 + ", " + spell.y2 + ", target NetId " + spell.targetNetId.ToString("x"));
                return true;
            }*/

            var futureProjNetId = Game.GetNewNetID();
            var spellNetId = Game.GetNewNetID();
            var targetObj = game.getMap().getObjectById(spell.targetNetId);
            var targetUnit = targetObj as Unit;

            var s = game.getPeerInfo(peer).getChampion().castSpell(spell.spellSlot, spell.x, spell.y, targetUnit, futureProjNetId, spellNetId);

            if (s == null)
                return false;

            var response = new CastSpellAns(s, spell.x, spell.y, futureProjNetId, spellNetId);
            PacketHandlerManager.getInstace().broadcastPacket(response, Channel.CHL_S2C);
            return true;
        }
    }
}
