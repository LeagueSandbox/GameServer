using GameServerCore.Enums;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.PacketDefinitions.Requests;
using LeaguePackets;
using LeaguePackets.Game;
using LeaguePackets.LoadScreen;
using PacketDefinitions420.Enums;
using PacketDefinitions420.PacketDefinitions.C2S;
using System;
using AttentionPingRequest = GameServerCore.Packets.PacketDefinitions.Requests.AttentionPingRequest;
using BuyItemRequest = GameServerCore.Packets.PacketDefinitions.Requests.BuyItemRequest;
using CastSpellRequest = GameServerCore.Packets.PacketDefinitions.Requests.CastSpellRequest;
using EmotionPacketRequest = GameServerCore.Packets.PacketDefinitions.Requests.EmotionPacketRequest;
using KeyCheckRequest = GameServerCore.Packets.PacketDefinitions.Requests.KeyCheckRequest;
using MovementRequest = GameServerCore.Packets.PacketDefinitions.Requests.MovementRequest;
using PingLoadInfoRequest = GameServerCore.Packets.PacketDefinitions.Requests.PingLoadInfoRequest;
using UpgradeSpellReq = GameServerCore.Packets.PacketDefinitions.Requests.UpgradeSpellReq;
using SwapItemsRequest = GameServerCore.Packets.PacketDefinitions.Requests.SwapItemsRequest;
using SynchVersionRequest = GameServerCore.Packets.PacketDefinitions.Requests.SynchVersionRequest;
using ViewRequest = GameServerCore.Packets.PacketDefinitions.Requests.ViewRequest;

namespace PacketDefinitions420
{
    /// <summary>
    /// Class which contains all functions which are called when handling packets sent from clients to the server (C2S).
    /// </summary>
    /// TODO: Remove all LeagueSandbox based PacketCmd usage and replace with LeaguePackets' GamePacketID enum.
    public class PacketReader
    {
        // this one only used in packet definitions, not exposed to the API currently, so no packet cmd assigned
        public static KeyCheckRequest ReadKeyCheckRequest(byte[] data)
        {
            var rq = new KeyCheckPacket();
            rq.Read(data);
            //var rq = new PacketDefinitions.C2S.KeyCheckRequest(data);
            return new KeyCheckRequest(rq.PlayerID, rq.ClientID, rq.VersionNumber, rq.CheckSum);
        }

        [PacketType(GamePacketID.SynchSimTimeC2S, Channel.CHL_GAMEPLAY)]
        public static SyncSimTimeRequest ReadSyncSimTimeRequest(byte[] data)
        {
            var sync = new SynchSimTimeC2S();
            sync.Read(data);
            return new SyncSimTimeRequest((int)sync.SenderNetID, sync.TimeLastClient, sync.TimeLastServer);
        }

        [PacketType(GamePacketID.RemoveItemReq)]
        public static SellItemRequest ReadSellItemRequest(byte[] data)
        {
            var rq = new SellItem(data);
            return new SellItemRequest(rq.NetId, rq.SlotId);
        }

        [PacketType(GamePacketID.ResumePacket)]
        public static UnpauseRequest ReadUnpauseRequest(byte[] data)
        {
            return new UnpauseRequest();
        }

        [PacketType(GamePacketID.C2S_QueryStatusReq)]
        public static QueryStatusRequest ReadQueryStatusRequest(byte[] data)
        {
            return new QueryStatusRequest();
        }

        [PacketType(GamePacketID.C2S_Ping_Load_Info)]
        public static PingLoadInfoRequest ReadPingLoadInfoRequest(byte[] data)
        {
            var rq = new PacketDefinitions.C2S.PingLoadInfoRequest(data);
            return new PingLoadInfoRequest(rq.NetId, rq.Position, rq.UserId, rq.Loaded, rq.Unk2, rq.Ping, rq.Unk3,
                rq.Unk4);
        }

        [PacketType(GamePacketID.SwapItemReq)]
        public static SwapItemsRequest ReadSwapItemsRequest(byte[] data)
        {
            var rq = new PacketDefinitions.C2S.SwapItemsRequest(data);
            return new SwapItemsRequest(rq.NetId, rq.SlotFrom, rq.SlotTo);
        }

        [PacketType(GamePacketID.World_SendCamera_Server)]
        public static ViewRequest ReadViewRequest(byte[] data)
        {
            var rq = new PacketDefinitions.C2S.ViewRequest(data);
            return new ViewRequest(rq.NetId, rq.X, rq.Zoom, rq.Y, rq.Y2, rq.Width, rq.Height, rq.Unk2, rq.RequestNo);
        }

        [PacketType(GamePacketID.NPC_UpgradeSpellReq)]
        public static UpgradeSpellReq ReadUpgradeSpellReq(byte[] data)
        {
            var rq = new NPC_UpgradeSpellReq();
            rq.Read(data);
            return new UpgradeSpellReq(rq.Slot, rq.IsEvolve);
        }

        [PacketType(GamePacketID.UseObjectC2S)]
        public static UseObjectRequest ReadUseObjectRequest(byte[] data)
        {
            var rq = new UseObject(data);
            return new UseObjectRequest(rq.NetId, rq.TargetNetId);
        }

        [PacketType(GamePacketID.C2S_UpdateGameOptions)]
        public static AutoAttackOptionRequest ReadAutoAttackOptionRequest(byte[] data)
        {
            var rq = new AutoAttackOption(data);
            return new AutoAttackOptionRequest(rq.Netid, rq.Activated == 1);
        }

        [PacketType(GamePacketID.C2S_PlayEmote)]
        public static EmotionPacketRequest ReadEmotionPacketRequest(byte[] data)
        {
            var rq = new PacketDefinitions.C2S.EmotionPacketRequest(data);

            // Convert packet emotion type to server emotion type
            // This is done so that when emotion ID changes in other packet versions, its functionality remains the same on server
            Emotions type;
            switch (rq.Id)
            {
                case EmotionType.DANCE:
                    type = Emotions.DANCE;
                    break;
                case EmotionType.TAUNT:
                    type = Emotions.TAUNT;
                    break;
                case EmotionType.LAUGH:
                    type = Emotions.LAUGH;
                    break;
                case EmotionType.JOKE:
                    type = Emotions.JOKE;
                    break;
                default:
                    type = Emotions.UNK;
                    break;
            }

            return new EmotionPacketRequest(rq.NetId, type);
        }

        [PacketType(GamePacketID.C2S_ClientReady)]
        public static StartGameRequest ReadStartGameRequest(byte[] data)
        {
            return new StartGameRequest();
        }

        [PacketType(GamePacketID.C2S_StatsUpdateReq)]
        public static ScoreboardRequest ReadScoreboardRequest(byte[] data)
        {
            return new ScoreboardRequest();
        }

        [PacketType(GamePacketID.C2S_MapPing)]
        public static AttentionPingRequest ReadAttentionPingRequest(byte[] data)
        {
            var rq = new PacketDefinitions.C2S.AttentionPingRequest(data);
            return new AttentionPingRequest(rq.X, rq.Y, rq.TargetNetId, rq.Type);
        }

        [PacketType(LoadScreenPacketID.RequestJoinTeam)]
        public static MapRequest ReadClientReadyRequest(byte[] data)
        {
            return new MapRequest();
        }

        [PacketType(LoadScreenPacketID.Chat, Channel.CHL_COMMUNICATION)]
        public static ChatMessageRequest ReadChatMessageRequest(byte[] data)
        {
            var rq = new Chat();
            rq.Read(data);
            return new ChatMessageRequest(rq.Message, (ChatType)rq.ChatType);
        }

        [PacketType(GamePacketID.C2S_OnTipEvent)]
        public static BlueTipClickedRequest ReadBlueTipRequest(byte[] data)
        {
            var rq = new BlueTipClicked(data);
            return new BlueTipClickedRequest(rq.Playernetid, rq.Netid);
        }

        [PacketType(GamePacketID.NPC_IssueOrderReq)]
        public static MovementRequest ReadMovementRequest(byte[] data)
        {
            var rq = new NPC_IssueOrderReq();
            rq.Read(data);
            // TODO: Verify if the MoveType cast works correctly.
            return new MovementRequest(rq.SenderNetID, rq.TargetNetID, rq.Position, (OrderType)rq.OrderType, rq.MovementData.Waypoints.ConvertAll(PacketExtensions.WaypointToVector2));
        }

        [PacketType(GamePacketID.Waypoint_Acc)]
        public static MoveConfirmRequest ReadMoveConfirmRequest(byte[] data)
        {
            return new MoveConfirmRequest();
        }

        [PacketType(GamePacketID.World_LockCamera_Server)]
        public static LockCameraRequest ReadCameraLockRequest(byte[] data)
        {
            return new LockCameraRequest();
        }

        [PacketType(GamePacketID.BuyItemReq)]
        public static BuyItemRequest ReadBuyItemRequest(byte[] data)
        {
            var rq = new PacketDefinitions.C2S.BuyItemRequest(data);
            return new BuyItemRequest(rq.NetId, rq.Id);
        }

        [PacketType(GamePacketID.C2S_Exit)]
        public static ExitRequest ReadExitRequest(byte[] data)
        {
            return new ExitRequest();
        }

        [PacketType(GamePacketID.NPC_CastSpellReq)]
        public static CastSpellRequest ReadCastSpellRequest(byte[] data)
        {
            var rq = new NPC_CastSpellReq();
            rq.Read(data);
            return new CastSpellRequest(rq.Slot, rq.IsSummonerSpellBook, rq.IsHudClickCast, rq.Position, rq.EndPosition, rq.TargetNetID);
        }

        [PacketType(GamePacketID.PausePacket)]
        public static PauseRequest ReadPauseGameRequest(byte[] data)
        {
            return new PauseRequest();
        }
        [PacketType(GamePacketID.C2S_TeamSurrenderVote)]
        public static SurrenderRequest ReadSurrenderRequest(byte[] data)
        {
            var rq = new C2S_TeamSurrenderVote();
            rq.Read(data);
            return new SurrenderRequest(rq.VotedYes);
        }

        [PacketType(GamePacketID.OnReplication_Acc)]
        public static StatsConfirmRequest ReadStatsConfirmRequest(byte[] data)
        {
            return new StatsConfirmRequest();
        }

        [PacketType(GamePacketID.SendSelectedObjID)]
        public static ClickRequest ReadClickRequest(byte[] data)
        {
            var rq = new Click(data);
            return new ClickRequest(rq.TargetNetId);
        }

        [PacketType(GamePacketID.SynchVersionC2S)]
        public static SynchVersionRequest ReadSynchVersionRequest(byte[] data)
        {
            var rq = new PacketDefinitions.C2S.SynchVersionRequest(data);
            return new SynchVersionRequest(rq.NetId, rq.ClientId, rq.Version);
        }

        [PacketType(GamePacketID.C2S_CharSelected)]
        public static SpawnRequest ReadSpawn(byte[] data)
        {
            return new SpawnRequest();
        }

        [PacketType(GamePacketID.C2S_OnQuestEvent)]
        public static QuestClickedRequest ReadQuestClickedRequest(byte[] data)
        {
            var rq = new QuestClicked(data);
            return new QuestClickedRequest(rq.Netid);
        }

        [PacketType(GamePacketID.C2S_SpellChargeUpdateReq)]
        public static SpellChargeUpdateReq ReadSpellChargeUpdateReq(byte[] data)
        {
            var rq = new C2S_SpellChargeUpdateReq();
            rq.Read(data);
            return new SpellChargeUpdateReq(rq.Slot, rq.IsSummonerSpellBook, rq.Position, rq.ForceStop);
        }
    }
}
