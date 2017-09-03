using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketArgs.DTO;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
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
        BasicTutorialMessageWindowArgs TranslateBasicTutorialMessageWindow(string message);
        BeginAutoAttackArgs TranslateBeginAutoAttack(Unit attacker, Unit attacked, uint futureProjNetId, bool isCritical);
        BuyItemResponseArgs TranslateBuyItemResponse(Unit actor, Item item, ItemOwnerType owner = ItemOwnerType.Champion);
        CastSpellResponseArgs TranslateCastSpellResponse(Spell spell, float x, float y, float xDragEnd, float yDragEnd, uint futureProjNetId, uint spellNetId);
        DamageDoneArgs TranslateDamageDone(Unit source, Unit target, float amount, DamageType type, DamageText damageText);
        DashArgs TranslateDash(Unit u, Target t, float dashSpeed, bool keepFacingLastDirection, float leapHeight = 0.0f, float followTargetMaxDistance = 0.0f, float backDistance = 0.0f, float travelTime = 0.0f);
        PacketObject TranslatePacketObject(GameObject obj);
        DestroyObjectArgs TranslateDestroyObjectArgs(Unit destroyer, Unit destroyed);
        EditBuffArgs TranslateEditBuff(Unit unit, byte slot, byte stacks);
        EnterVisionAgainArgs TranslateEnterVisionAgain(ObjAIBase m);
        SpawnProjectileArgs TranslateSpawnProjectile(Projectile p);
        FogUpdate2Args TranslateFogUpdate2(Unit unit, uint fogNetId);
        HeroSpawnArgs TranslateHeroSpawn(ClientInfo player, int playerId);
        HeroSpawn2Args TranslateHeroSpawn2(Champion champion);
    }
}
