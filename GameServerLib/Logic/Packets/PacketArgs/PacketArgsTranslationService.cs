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
    public class PacketArgsTranslationService : IPacketArgsTranslationService
    {
        public AddBuffArgs TranslateAddBuff(Buff buff)
        {
            if (buff == null)
                return default(AddBuffArgs);

            var target = buff.TargetUnit?.NetId ?? default(uint);
            var source = buff.SourceUnit?.NetId ?? default(uint);
            return new AddBuffArgs(target, source, buff.Stacks, buff.Duration, buff.BuffType, buff.Name, buff.Slot);
        }

        public AddGoldArgs TranslateAddGold(Champion acceptor, Unit donor, float amount)
        {
            if (acceptor == null)
                return default(AddGoldArgs);

            var acceptorNetId = acceptor.NetId;
            var donorNetId = donor?.NetId ?? default(uint);
            return new AddGoldArgs(acceptorNetId, donorNetId, amount);
        }

        public AddUnitFOWArgs TranslateAddUnitFow(Unit unit)
        {
            if (unit == null)
                return default(AddUnitFOWArgs);

            var unitNetId = unit.NetId;
            return new AddUnitFOWArgs(unitNetId);
        }

        public AddXpArgs TranslateAddXp(Champion unit, float amount)
        {
            if (unit == null)
                return default(AddXpArgs);

            var targetNetId = unit.NetId;
            return new AddXpArgs(targetNetId, amount);
        }

        public AnnounceArgs TranslateAnnounce(Announces type, int mapId)
        {
            return new AnnounceArgs(type, mapId);
        }

        public AttachMinimapIconArgs TranslateAttachMinimapIconArgs(Unit unit, byte unk1, string iconName, byte unk2, string unk3, string unk4)
        {
            if (unit == null)
                return default(AttachMinimapIconArgs);

            var netId = unit.NetId;
            return new AttachMinimapIconArgs(netId, unk1, iconName, unk2, unk3, unk4);
        }

        public AttentionPingResponseArgs TranslateAttentionPingResponse(ClientInfo player, float x, float y, uint targetNetId, Pings type)
        {
            if (player == null)
                return default(AttentionPingResponseArgs);

            var playerNetId = player.Champion?.NetId ?? default(uint);
            return new AttentionPingResponseArgs(playerNetId, x, y, targetNetId, type);
        }

        public AvatarInfoArgs TranslateAvatarInfo(ClientInfo clientInfo)
        {
            if (clientInfo == null)
                return default(AvatarInfoArgs);

            var netId = clientInfo.Champion.NetId;
            var spell1 = string.Empty;
            var spell2 = string.Empty;
            if (clientInfo.SummonerSkills?.Length > 1)
            {
                spell1 = clientInfo.SummonerSkills[0];
                spell2 = clientInfo.SummonerSkills[1];
            }

            return new AvatarInfoArgs(netId, clientInfo.Champion.RuneList, spell1, spell2);
        }
    }
}
