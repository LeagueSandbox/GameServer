using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketArgs
{
    public interface IPacketArgsTranslationService
    {
        AddBuffArgs TranslateAddBuff(Buff buff);
        AddGoldArgs TranslateAddGold(Champion acceptor, Unit donor, float amount);
        AddUnitFOWArgs TranslateAddUnitFow(Unit unit);
        AddXpArgs TranslateAddXp(Champion unit, float amount);
        AnnounceArgs TranslateAnnounce(Announces type, int mapId);
        AttachMinimapIconArgs TranslateAttachMinimapIconArgs(Unit unit, byte unk1, string iconName, byte unk2, string unk3, string unk4);
        AttentionPingResponseArgs TranslateAttentionPingResponse(ClientInfo player, float x, float y, uint targetNetId, Pings type);
        AvatarInfoArgs TranslateAvatarInfo(ClientInfo clientInfo);
    }
}
