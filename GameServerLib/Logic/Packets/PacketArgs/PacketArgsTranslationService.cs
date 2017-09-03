using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Maps;
using LeagueSandbox.GameServer.Logic.Packets.PacketArgs.DTO;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketArgs
{
    public class PacketArgsTranslationService : IPacketArgsTranslationService
    {
        //TODO: constructor injection when possible
        private Game _game;
        private Game Game => _game ?? (_game = Program.ResolveDependency<Game>());

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

        public AttachMinimapIconArgs TranslateAttachMinimapIconArgs(Unit unit, byte unk1, string iconName, byte unk2,
            string unk3, string unk4)
        {
            if (unit == null)
                return default(AttachMinimapIconArgs);

            var netId = unit.NetId;
            return new AttachMinimapIconArgs(netId, unk1, iconName, unk2, unk3, unk4);
        }

        public AttentionPingResponseArgs TranslateAttentionPingResponse(ClientInfo player, float x, float y,
            uint targetNetId, Pings type)
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

        public BasicTutorialMessageWindowArgs TranslateBasicTutorialMessageWindow(string message)
        {
            return new BasicTutorialMessageWindowArgs(message);
        }

        public BeginAutoAttackArgs TranslateBeginAutoAttack(Unit attacker, Unit attacked, uint futureProjNetId,
            bool isCritical)
        {
            if (attacker == null || attacked == null)
                return default(BeginAutoAttackArgs);

            var attackerUnit = new UnitAtLocation(attacker.NetId, attacker.X, attacker.Y, attacker.GetZ());
            var attackedUnit = new UnitAtLocation(attacked.NetId, attacked.X, attacked.Y, attacked.GetZ());
            return new BeginAutoAttackArgs(attackerUnit, attackedUnit, futureProjNetId, isCritical);
        }

        public BuyItemResponseArgs TranslateBuyItemResponse(Unit actor, Item item,
            ItemOwnerType owner = ItemOwnerType.Champion)
        {
            if (actor == null || item == null)
                return default(BuyItemResponseArgs);

            var itemId = item.ItemType.ItemId;
            var slot = actor.Inventory.GetItemSlot(item);
            return new BuyItemResponseArgs(actor.NetId, itemId, slot, item.StackSize, ItemOwnerType.Champion);
        }

        public CastSpellResponseArgs TranslateCastSpellResponse(Spell spell, float x, float y, float xDragEnd,
            float yDragEnd, uint futureProjNetId, uint spellNetId)
        {
            if (spell == null)
                return default(CastSpellResponseArgs);

            var castTime = spell.SpellData.GetCastTime();
            var sOwner = spell.Owner;
            var ownerHash = sOwner.getChampionHash();
            var owner = new ChampionAtLocation(sOwner.NetId, ownerHash, sOwner.X, sOwner.Y, sOwner.GetZ());
            var mpCost = spell.SpellData.ManaCost[spell.Level];
            return new CastSpellResponseArgs(spell.GetHash(), (byte)spell.Level, castTime, spell.getCooldown(),
                spell.Slot, mpCost, owner, x, y, xDragEnd, yDragEnd, futureProjNetId, spellNetId);
        }

        public DamageDoneArgs TranslateDamageDone(Unit source, Unit target, float amount, DamageType type,
            DamageText damageText)
        {
            if (target == null)
                return default(DamageDoneArgs);

            var sourceNetId = source?.NetId ?? default(uint);
            var targetNetId = target.NetId;
            return new DamageDoneArgs(sourceNetId, targetNetId, amount, type, damageText);
        }

        public DashArgs TranslateDash(Unit u, Target t, float dashSpeed, bool keepFacingLastDirection,
            float leapHeight = 0, float followTargetMaxDistance = 0, float backDistance = 0, float travelTime = 0)
        {
            if (u == null || t == null)
                return new DashArgs();

            var unit = new UnitAtLocation(u.NetId, u.X, u.Y, u.GetZ());
            var target = new UnitAtLocation(t.IsSimpleTarget ? 0u : ((GameObject)t).NetId, t.X, t.Y, 0f);
            return new DashArgs(unit, target, dashSpeed, keepFacingLastDirection, leapHeight, followTargetMaxDistance,
                backDistance, travelTime);
        }

        public PacketObject TranslatePacketObject(GameObject obj)
        {
            if (obj == null)
                return new PacketObject();

            return new PacketObject(obj.NetId);
        }

        public DestroyObjectArgs TranslateDestroyObjectArgs(Unit destroyer, Unit destroyed)
        {
            if (destroyer == null || destroyed == null)
                return new DestroyObjectArgs();

            var destroyerUnit = TranslatePacketObject(destroyer);
            var destroyedUnit = TranslatePacketObject(destroyed);
            return new DestroyObjectArgs(destroyerUnit, destroyedUnit);
        }

        public EditBuffArgs TranslateEditBuff(Unit unit, byte slot, byte stacks)
        {
            if (unit == null)
                return new EditBuffArgs();

            var packetObject = new PacketObject(unit.NetId);
            return new EditBuffArgs(packetObject, slot, stacks);
        }

        public EnterVisionAgainArgs TranslateEnterVisionAgain(ObjAIBase m)
        {
            if (m == null)
                return new EnterVisionAgainArgs();

            var obj = new UnitAtLocation(m.NetId, m.X, m.Y, m.GetZ());
            return new EnterVisionAgainArgs(obj, m.Waypoints.ToList(), m.CurWaypoint);
        }

        public SpawnProjectileArgs TranslateSpawnProjectile(Projectile p)
        {
            if (p == null)
                return new SpawnProjectileArgs();

            var projectile = new UnitAtLocation(p.NetId, p.X, p.Y, p.GetZ());

            var targetZ = Game.Map.NavGrid.GetHeightAtLocation(p.Target.X, p.Target.Y);
            var targetNetId = p.Target.IsSimpleTarget ? 0u : ((GameObject)p.Target).NetId;
            var target = new UnitAtLocation(targetNetId, p.Target.X, p.Target.Y, targetZ);

            var ownerChampion = p.Owner as Champion;
            var ownerHash = ownerChampion?.getChampionHash() ?? 0;
            var owner = new ChampionAtLocation(p.Owner.NetId, ownerHash, p.Owner.X, p.Owner.Y, p.Owner.GetZ());

            return new SpawnProjectileArgs(projectile, target, p.Target.IsSimpleTarget, p.getMoveSpeed(),
                p.ProjectileId, owner);
        }

        public FogUpdate2Args TranslateFogUpdate2(Unit unit, uint fogNetId)
        {
            if (unit == null)
                return new FogUpdate2Args();

            var u = new UnitAtLocation(unit.NetId, unit.X, unit.Y, unit.GetZ());
            return new FogUpdate2Args(u, unit.Team, unit.VisionRadius, fogNetId);
        }

        public HeroSpawnArgs TranslateHeroSpawn(ClientInfo player, int playerId)
        {
            if (player?.Champion == null)
                return new HeroSpawnArgs();


            return new HeroSpawnArgs(player.Champion.NetId, playerId, player.Team, player.SkinNo, player.Name,
                player.Champion.Model);
        }

        public HeroSpawn2Args TranslateHeroSpawn2(Champion champion)
        {
            if (champion == null)
                return new HeroSpawn2Args();

            return new HeroSpawn2Args(new UnitAtLocation(champion.NetId, champion.X, champion.Y, champion.GetZ()));
        }
    }
}
