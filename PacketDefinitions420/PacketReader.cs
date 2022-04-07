using GameServerCore.Packets.Enums;
using LeaguePackets;
using LeaguePackets.Game;
using LeaguePackets.LoadScreen;

namespace PacketDefinitions420
{
    /// <summary>
    /// Class which contains all functions which are called when handling packets sent from clients to the server (C2S).
    /// </summary>
    /// TODO: Remove all LeagueSandbox based PacketCmd usage and replace with LeaguePackets' GamePacketID enum.
    public class PacketReader
    {
        // this one only used in packet definitions, not exposed to the API currently, so no packet cmd assigned
        public static KeyCheckPacket ReadKeyCheckRequest(byte[] data)
        {
            var rq = new KeyCheckPacket();
            rq.Read(data);
            return rq;
        }

        [PacketType(GamePacketID.SynchSimTimeC2S, Channel.CHL_GAMEPLAY)]
        public static SynchSimTimeC2S ReadSyncSimTimeRequest(byte[] data)
        {
            var rq = new SynchSimTimeC2S();
            rq.Read(data);
            return rq;
        }

        [PacketType(GamePacketID.RemoveItemReq)]
        public static RemoveItemReq ReadSellItemRequest(byte[] data)
        {
            var rq = new RemoveItemReq();
            rq.Read(data);
            return rq;
        }

        [PacketType(GamePacketID.ResumePacket)]
        public static ResumePacket ReadUnpauseRequest(byte[] data)
        {
            var rq = new ResumePacket();
            rq.Read(data);
            return rq;
        }

        [PacketType(GamePacketID.C2S_QueryStatusReq)]
        public static C2S_QueryStatusReq ReadQueryStatusRequest(byte[] data)
        {
            var rq = new C2S_QueryStatusReq();
            rq.Read(data);
            return rq;
        }

        [PacketType(GamePacketID.C2S_Ping_Load_Info)]
        public static C2S_Ping_Load_Info ReadPingLoadInfoRequest(byte[] data)
        {
            var rq = new C2S_Ping_Load_Info();
            rq.Read(data);
            return rq;
        }

        [PacketType(GamePacketID.SwapItemReq)]
        public static SwapItemReq ReadSwapItemsRequest(byte[] data)
        {
            var rq = new SwapItemReq();
            rq.Read(data);
            return rq;
        }

        [PacketType(GamePacketID.World_SendCamera_Server)]
        public static World_SendCamera_Server ReadViewRequest(byte[] data)
        {
            var rq = new World_SendCamera_Server();
            rq.Read(data);
            return rq;
        }

        [PacketType(GamePacketID.NPC_UpgradeSpellReq)]
        public static NPC_UpgradeSpellReq ReadUpgradeSpellReq(byte[] data)
        {
            var rq = new NPC_UpgradeSpellReq();
            rq.Read(data);
            return rq;
        }

        [PacketType(GamePacketID.UseObjectC2S)]
        public static UseObjectC2S ReadUseObjectRequest(byte[] data)
        {
            var rq = new UseObjectC2S();
            rq.Read(data);
            return rq;
        }

        [PacketType(GamePacketID.C2S_UpdateGameOptions)]
        public static C2S_UpdateGameOptions ReadAutoAttackOptionRequest(byte[] data)
        {
            var rq = new C2S_UpdateGameOptions();
            rq.Read(data);
            return rq;
        }

        [PacketType(GamePacketID.C2S_PlayEmote)]
        public static C2S_PlayEmote ReadEmotionPacketRequest(byte[] data)
        {
            var rq = new C2S_PlayEmote();
            rq.Read(data);
            return rq;
        }

        [PacketType(GamePacketID.C2S_ClientReady)]
        public static C2S_ClientReady ReadStartGameRequest(byte[] data)
        {
            var rq = new C2S_ClientReady();
            rq.Read(data);
            return rq;
        }

        [PacketType(GamePacketID.C2S_StatsUpdateReq)]
        public static C2S_StatsUpdateReq ReadScoreboardRequest(byte[] data)
        {
            var rq = new C2S_StatsUpdateReq();
            rq.Read(data);
            return rq;
        }

        [PacketType(GamePacketID.C2S_MapPing)]
        public static C2S_MapPing ReadAttentionPingRequest(byte[] data)
        {
            var rq = new C2S_MapPing();
            rq.Read(data);
            return rq;
        }

        [PacketType(LoadScreenPacketID.RequestJoinTeam)]
        public static RequestJoinTeam ReadClientJoinTeamRequest(byte[] data)
        {
            var rq = new RequestJoinTeam();
            rq.Read(data);
            return rq;
        }

        [PacketType(LoadScreenPacketID.Chat, Channel.CHL_COMMUNICATION)]
        public static Chat ReadChatMessageRequest(byte[] data)
        {
            var rq = new Chat();
            rq.Read(data);
            return rq;
        }

        [PacketType(GamePacketID.C2S_OnTipEvent)]
        public static C2S_OnTipEvent ReadBlueTipRequest(byte[] data)
        {
            var rq = new C2S_OnTipEvent();
            rq.Read(data);
            return rq;
        }

        [PacketType(GamePacketID.NPC_IssueOrderReq)]
        public static NPC_IssueOrderReq ReadMovementRequest(byte[] data)
        {
            var rq = new NPC_IssueOrderReq();
            rq.Read(data);
            var test = rq.MovementData.Waypoints.ConvertAll(PacketExtensions.WaypointToVector2);
            return rq;
        }

        [PacketType(GamePacketID.Waypoint_Acc)]
        public static Waypoint_Acc ReadMoveConfirmRequest(byte[] data)
        {
            var rq = new Waypoint_Acc();
            rq.Read(data);
            return rq;
        }

        [PacketType(GamePacketID.World_LockCamera_Server)]
        public static World_LockCamera_Server ReadCameraLockRequest(byte[] data)
        {
            var rq = new World_LockCamera_Server();
            rq.Read(data);
            return rq;
        }

        [PacketType(GamePacketID.BuyItemReq)]
        public static BuyItemReq ReadBuyItemRequest(byte[] data)
        {
            var rq = new BuyItemReq();
            rq.Read(data);
            return rq;
        }

        [PacketType(GamePacketID.C2S_Exit)]
        public static C2S_Exit ReadExitRequest(byte[] data)
        {
            var rq = new C2S_Exit();
            rq.Read(data);
            return rq;
        }

        [PacketType(GamePacketID.NPC_CastSpellReq)]
        public static NPC_CastSpellReq ReadCastSpellRequest(byte[] data)
        {
            var rq = new NPC_CastSpellReq();
            rq.Read(data);
            return rq;
        }

        [PacketType(GamePacketID.PausePacket)]
        public static PausePacket ReadPauseGameRequest(byte[] data)
        {
            var rq = new PausePacket();
            rq.Read(data);
            return rq;
        }
        [PacketType(GamePacketID.C2S_TeamSurrenderVote)]
        public static C2S_TeamSurrenderVote ReadSurrenderRequest(byte[] data)
        {
            var rq = new C2S_TeamSurrenderVote();
            rq.Read(data);
            return rq;
        }

        [PacketType(GamePacketID.OnReplication_Acc)]
        public static OnReplication_Acc ReadStatsConfirmRequest(byte[] data)
        {
            var rq = new OnReplication_Acc();
            rq.Read(data);
            return rq;
        }

        [PacketType(GamePacketID.SendSelectedObjID)]
        public static SendSelectedObjID ReadClickRequest(byte[] data)
        {
            var rq = new SendSelectedObjID();
            rq.Read(data);
            return rq;
        }

        [PacketType(GamePacketID.SynchVersionC2S)]
        public static SynchVersionC2S ReadSynchVersionRequest(byte[] data)
        {
            var rq = new SynchVersionC2S();
            rq.Read(data);
            return rq;
        }

        [PacketType(GamePacketID.C2S_CharSelected)]
        public static C2S_CharSelected ReadSpawn(byte[] data)
        {
            var rq = new C2S_CharSelected();
            rq.Read(data);
            return rq;
        }

        [PacketType(GamePacketID.C2S_OnQuestEvent)]
        public static C2S_OnQuestEvent ReadQuestClickedRequest(byte[] data)
        {
            var rq = new C2S_OnQuestEvent();
            rq.Read(data);
            return rq;
        }

        [PacketType(GamePacketID.C2S_SpellChargeUpdateReq)]
        public static C2S_SpellChargeUpdateReq ReadSpellChargeUpdateReq(byte[] data)
        {
            var rq = new C2S_SpellChargeUpdateReq();
            rq.Read(data);
            return rq;
        }
    }
}
