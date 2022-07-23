using GameServerCore.Packets.PacketDefinitions.Requests;
using GameServerCore.Packets.Enums;
using LeaguePackets;
using LeaguePackets.Game;
using LeaguePackets.LoadScreen;
using GameServerCore.Enums;
using System;
using static PacketDefinitions420.PacketExtensions;

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
            return new KeyCheckRequest(rq.PlayerID, rq.ClientID, rq.VersionNumber, rq.CheckSum);
        }

        [PacketType(GamePacketID.SynchSimTimeC2S, Channel.CHL_GAMEPLAY)]
        public static SyncSimTimeRequest ReadSyncSimTimeRequest(byte[] data)
        {
            var rq = new SynchSimTimeC2S();
            rq.Read(data);
            return new SyncSimTimeRequest(rq.TimeLastServer, rq.TimeLastClient);
        }

        [PacketType(GamePacketID.RemoveItemReq)]
        public static SellItemRequest ReadSellItemRequest(byte[] data)
        {
            var rq = new RemoveItemReq();
            rq.Read(data);
            return new SellItemRequest(rq.Slot, rq.Sell);
        }

        [PacketType(GamePacketID.ResumePacket)]
        public static UnpauseRequest ReadUnpauseRequest(byte[] data)
        {
            var rq = new ResumePacket();
            rq.Read(data);
            return new UnpauseRequest(rq.ClientID, rq.Delayed);
        }

        [PacketType(GamePacketID.C2S_QueryStatusReq)]
        public static QueryStatusRequest ReadQueryStatusRequest(byte[] data)
        {
            var rq = new C2S_QueryStatusReq();
            rq.Read(data);
            return new QueryStatusRequest();
        }

        [PacketType(GamePacketID.C2S_Ping_Load_Info)]
        public static PingLoadInfoRequest ReadPingLoadInfoRequest(byte[] data)
        {
            var rq = new C2S_Ping_Load_Info();
            rq.Read(data);
            return new PingLoadInfoRequest(rq.ConnectionInfo.ClientID, rq.ConnectionInfo.PlayerID, rq.ConnectionInfo.Percentage, rq.ConnectionInfo.ETA, rq.ConnectionInfo.Count, rq.ConnectionInfo.Ping, rq.ConnectionInfo.Ready);
        }

        [PacketType(GamePacketID.SwapItemReq)]
        public static SwapItemsRequest ReadSwapItemsRequest(byte[] data)
        {
            var rq = new SwapItemReq();
            rq.Read(data);
            return new SwapItemsRequest(rq.Source, rq.Destination);
        }

        [PacketType(GamePacketID.World_SendCamera_Server)]
        public static ViewRequest ReadViewRequest(byte[] data)
        {
            var rq = new World_SendCamera_Server();
            rq.Read(data);
            return new ViewRequest(rq.CameraPosition, rq.CameraDirection, rq.ClientID, rq.SyncID);
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
            var rq = new UseObjectC2S();
            rq.Read(data);
            return new UseObjectRequest(rq.TargetNetID);
        }

        [PacketType(GamePacketID.C2S_UpdateGameOptions)]
        public static AutoAttackOptionRequest ReadAutoAttackOptionRequest(byte[] data)
        {
            var rq = new C2S_UpdateGameOptions();
            rq.Read(data);
            return new AutoAttackOptionRequest(rq.AutoAttackEnabled);
        }

        [PacketType(GamePacketID.C2S_PlayEmote)]
        public static EmotionPacketRequest ReadEmotionPacketRequest(byte[] data)
        {
            var rq = new C2S_PlayEmote();
            rq.Read(data);
            return new EmotionPacketRequest((Emotions)rq.EmoteID);
        }

        [PacketType(GamePacketID.C2S_ClientReady)]
        public static StartGameRequest ReadStartGameRequest(byte[] data)
        {
            var rq = new C2S_ClientReady();
            rq.Read(data);
            return new StartGameRequest(rq.TipConfig.TipID, rq.TipConfig.ColorID, rq.TipConfig.DurationID, rq.TipConfig.Flags);
        }

        [PacketType(GamePacketID.C2S_StatsUpdateReq)]
        public static ScoreboardRequest ReadScoreboardRequest(byte[] data)
        {
            var rq = new C2S_StatsUpdateReq();
            rq.Read(data);
            return new ScoreboardRequest();
        }

        [PacketType(GamePacketID.C2S_MapPing)]
        public static AttentionPingRequest ReadAttentionPingRequest(byte[] data)
        {
            var rq = new C2S_MapPing();
            rq.Read(data);
            return new AttentionPingRequest(rq.Position, rq.TargetNetID, (Pings)rq.PingCategory);
        }

        [PacketType(LoadScreenPacketID.RequestJoinTeam)]
        public static JoinTeamRequest ReadClientJoinTeamRequest(byte[] data)
        {
            var rq = new RequestJoinTeam();
            rq.Read(data);
            return new JoinTeamRequest(rq.ClientID, rq.NetTeamID);
        }

        [PacketType(LoadScreenPacketID.Chat, Channel.CHL_COMMUNICATION)]
        public static ChatMessageRequest ReadChatMessageRequest(byte[] data)
        {
            var rq = new Chat();
            rq.Read(data);
            return new ChatMessageRequest(rq.Message, (ChatType)rq.ChatType, rq.Params, rq.Localized, rq.NetID, rq.ClientID);
        }

        [PacketType(GamePacketID.C2S_OnTipEvent)]
        public static BlueTipClickedRequest ReadBlueTipRequest(byte[] data)
        {
            var rq = new C2S_OnTipEvent();
            rq.Read(data);
            return new BlueTipClickedRequest((TipCommand)rq.TipCommand, rq.TipID);
        }

        [PacketType(GamePacketID.NPC_IssueOrderReq)]
        public static MovementRequest ReadMovementRequest(byte[] data)
        {
            var rq = new NPC_IssueOrderReq();
            rq.Read(data);
            var test = rq.MovementData.Waypoints.ConvertAll(WaypointToVector2);
            return new MovementRequest((OrderType)rq.OrderType, rq.Position, rq.TargetNetID, rq.MovementData.TeleportNetID, rq.MovementData.HasTeleportID, rq.MovementData.Waypoints.ConvertAll(WaypointToVector2));
        }

        [PacketType(GamePacketID.Waypoint_Acc)]
        public static MoveConfirmRequest ReadMoveConfirmRequest(byte[] data)
        {
            var rq = new Waypoint_Acc();
            rq.Read(data);
            return new MoveConfirmRequest(rq.SyncID, rq.TeleportCount);
        }

        [PacketType(GamePacketID.World_LockCamera_Server)]
        public static LockCameraRequest ReadCameraLockRequest(byte[] data)
        {
            var rq = new World_LockCamera_Server();
            rq.Read(data);
            return new LockCameraRequest(rq.Locked, rq.ClientID);
        }

        [PacketType(GamePacketID.BuyItemReq)]
        public static BuyItemRequest ReadBuyItemRequest(byte[] data)
        {
            var rq = new BuyItemReq();
            rq.Read(data);
            return new BuyItemRequest(rq.ItemID);
        }

        [PacketType(GamePacketID.C2S_Exit)]
        public static ExitRequest ReadExitRequest(byte[] data)
        {
            var rq = new C2S_Exit();
            rq.Read(data);
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
            var rq = new PausePacket();
            rq.Read(data);
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
        public static ReplicationConfirmRequest ReadStatsConfirmRequest(byte[] data)
        {
            var rq = new OnReplication_Acc();
            rq.Read(data);
            return new ReplicationConfirmRequest((uint)Environment.TickCount);
        }

        [PacketType(GamePacketID.SendSelectedObjID)]
        public static ClickRequest ReadClickRequest(byte[] data)
        {
            var rq = new SendSelectedObjID();
            rq.Read(data);
            return new ClickRequest(rq.SelectedNetID, rq.ClientID);
        }

        [PacketType(GamePacketID.SynchVersionC2S)]
        public static SynchVersionRequest ReadSynchVersionRequest(byte[] data)
        {
            var rq = new SynchVersionC2S();
            rq.Read(data);
            return new SynchVersionRequest(rq.ClientID, rq.Version);
        }

        [PacketType(GamePacketID.C2S_CharSelected)]
        public static SpawnRequest ReadSpawn(byte[] data)
        {
            var rq = new C2S_CharSelected();
            rq.Read(data);
            return new SpawnRequest();
        }

        [PacketType(GamePacketID.C2S_OnQuestEvent)]
        public static QuestClickedRequest ReadQuestClickedRequest(byte[] data)
        {
            var rq = new C2S_OnQuestEvent();
            rq.Read(data);
            return new QuestClickedRequest(rq.QuestID, (QuestEvent)rq.QuestEvent);
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
