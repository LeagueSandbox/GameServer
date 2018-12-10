using GameServerCore.Packets.Enums;
using GameServerCore.Packets.PacketDefinitions.Requests;
using PacketDefinitions420.Enums;
using PacketDefinitions420.PacketDefinitions;
using PacketDefinitions420.PacketDefinitions.C2S;
using System;
using AttentionPingRequest = GameServerCore.Packets.PacketDefinitions.Requests.AttentionPingRequest;
using BuyItemRequest = GameServerCore.Packets.PacketDefinitions.Requests.BuyItemRequest;
using CastSpellRequest = GameServerCore.Packets.PacketDefinitions.Requests.CastSpellRequest;
using EmotionPacketRequest = GameServerCore.Packets.PacketDefinitions.Requests.EmotionPacketRequest;
using KeyCheckRequest = GameServerCore.Packets.PacketDefinitions.Requests.KeyCheckRequest;
using MovementRequest = GameServerCore.Packets.PacketDefinitions.Requests.MovementRequest;
using PingLoadInfoRequest = GameServerCore.Packets.PacketDefinitions.Requests.PingLoadInfoRequest;
using SkillUpRequest = GameServerCore.Packets.PacketDefinitions.Requests.SkillUpRequest;
using SwapItemsRequest = GameServerCore.Packets.PacketDefinitions.Requests.SwapItemsRequest;
using SynchVersionRequest = GameServerCore.Packets.PacketDefinitions.Requests.SynchVersionRequest;
using ViewRequest = GameServerCore.Packets.PacketDefinitions.Requests.ViewRequest;

namespace PacketDefinitions420
{
    public class PacketReader
    {
        [PacketType(PacketCmd.PKT_C2S_EXIT)]
        public static ExitRequest ReadExitRequest(byte[] data)
        {
            return new ExitRequest();
        }
        [PacketType(PacketCmd.PKT_C2S_LOCK_CAMERA)]
        public static LockCameraRequest ReadCamerLockRequest(byte[] data)
        {
            return new LockCameraRequest();
        }
        [PacketType(PacketCmd.PKT_C2S_CLIENT_READY, Channel.CHL_LOADING_SCREEN)]
        public static MapRequest ReadClientReadyRequest(byte[] data)
        {
            return new MapRequest();
        }
        [PacketType(PacketCmd.PKT_C2S_MOVE_CONFIRM)]
        public static MoveConfirmRequest ReadMoveConfirmRequest(byte[] data)
        {
            return new MoveConfirmRequest();
        }
        [PacketType(PacketCmd.PKT_PAUSE_GAME)]
        public static PauseRequest ReadPauseGameRequest(byte[] data)
        {
            return new PauseRequest();
        }
        [PacketType(PacketCmd.PKT_C2S_QUERY_STATUS_REQ)]
        public static QueryStatusRequest ReadQueryStatusRequest(byte[] data)
        {
            return new QueryStatusRequest();
        }
        [PacketType(PacketCmd.PKT_C2S_SCOREBOARD)]
        public static ScoreboardRequest ReadScoreboardRequest(byte[] data)
        {
            return new ScoreboardRequest();
        }
        [PacketType(PacketCmd.PKT_C2S_CHAR_LOADED)]
        public static SpawnRequest ReadSpawn(byte[] data)
        {
            return new SpawnRequest();
        }
        [PacketType(PacketCmd.PKT_C2S_START_GAME)]
        public static StartGameRequest ReadStartGameRequest(byte[] data)
        {
            return new StartGameRequest();
        }
        [PacketType(PacketCmd.PKT_C2S_STATS_CONFIRM)]
        public static StatsConfirmRequest ReadStatsConfirmRequest(byte[] data)
        {
            return new StatsConfirmRequest();
        }
        [PacketType(PacketCmd.PKT_C2S_SURRENDER)]
        public static SurrenderRequest ReadSurrenderRequest(byte[] data)
        {
            return new SurrenderRequest();
        }
        [PacketType(PacketCmd.PKT_UNPAUSE_GAME)]
        public static UnpauseRequest ReadUnpauseRequest(byte[] data)
        {
            return new UnpauseRequest();
        }

        [PacketType(PacketCmd.PKT_C2S_SWAP_ITEMS)]
        public static SwapItemsRequest ReadSwapItemsRequest(byte[] data)
        {
            var rq = new PacketDefinitions.C2S.SwapItemsRequest(data);
            return new SwapItemsRequest(rq.NetId, rq.SlotFrom, rq.SlotTo);
        }

        [PacketType(PacketCmd.PKT_C2S_ATTENTION_PING)]
        public static AttentionPingRequest ReadAttentionPingRequest(byte[] data)
        {
            var rq = new PacketDefinitions.C2S.AttentionPingRequest(data);
            return new AttentionPingRequest(rq.X, rq.Y, rq.TargetNetId, rq.Type);
        }

        [PacketType(PacketCmd.PKT_C2S_AUTO_ATTACK_OPTION)]
        public static AutoAttackOptionRequest ReadAutoAttackOptionRequest(byte[] data)
        {
            var rq = new AutoAttackOption(data);
            return new AutoAttackOptionRequest(rq.Netid, rq.Activated == 1);
        }

        [PacketType(PacketCmd.PKT_C2S_BLUE_TIP_CLICKED)]
        public static BlueTipClickedRequest ReadBlueTipRequest(byte[] data)
        {
            var rq = new BlueTipClicked(data);
            return new BlueTipClickedRequest(rq.Playernetid, rq.Netid);
        }

        [PacketType(PacketCmd.PKT_C2S_BUY_ITEM_REQ)]
        public static BuyItemRequest ReadBuyItemRequest(byte[] data)
        {
            var rq = new PacketDefinitions.C2S.BuyItemRequest(data);
            return new BuyItemRequest(rq.NetId, rq.Id);
        }

        [PacketType(PacketCmd.PKT_C2S_CAST_SPELL)]
        public static CastSpellRequest ReadCastSpellRequest(byte[] data)
        {
            var rq = new PacketDefinitions.C2S.CastSpellRequest(data);
            return new CastSpellRequest(rq.NetId, rq.SpellSlot, rq.X, rq.Y, rq.X2, rq.Y2, rq.TargetNetId);
        }

        [PacketType(PacketCmd.PKT_CHAT_BOX_MESSAGE, Channel.CHL_COMMUNICATION)]
        public static ChatMessageRequest ReadChatMessageRequest(byte[] data)
        {
            var rq = new ChatMessage(data);
            return new ChatMessageRequest(rq.Msg, rq.Type);
        }

        [PacketType(PacketCmd.PKT_C2S_CLICK)]
        public static ClickRequest ReadClickRequest(byte[] data)
        {
            var rq = new Click(data);
            return new ClickRequest(rq.TargetNetId);
        }

        [PacketType(PacketCmd.PKT_C2S_CURSOR_POSITION_ON_WORLD)]
        public static CursorPositionOnWorldRequest ReadCursorPositionOnWorldRequest(byte[] data)
        {
            var rq = new CursorPositionOnWorld(data);
            return new CursorPositionOnWorldRequest(rq.NetId, rq.Unk1, rq.X, rq.Z, rq.Y);
        }

        [PacketType(PacketCmd.PKT_C2S_EMOTION)]
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

        [PacketType(PacketCmd.PKT_C2S_HEART_BEAT, Channel.CHL_GAMEPLAY)]
        public static HeartbeatRequest ReadHeartbeatRequest(byte[] data)
        {
            var rq = new HeartBeat(data);
            return new HeartbeatRequest(rq.NetId, rq.ReceiveTime, rq.AckTime);
        }

        // this one only used in packet definitions, not exposed to the API currently, so no packet cmd assigned
        public static KeyCheckRequest ReadKeyCheckRequest(byte[] data)
        {
            var rq = new PacketDefinitions.C2S.KeyCheckRequest(data);
            return new KeyCheckRequest(rq.PlayerNo, rq.UserId, rq.VersionNo, rq.CheckId);
        }

        [PacketType(PacketCmd.PKT_C2S_PING_LOAD_INFO)]
        public static PingLoadInfoRequest ReadPingLoadInfoRequest(byte[] data)
        {
            var rq = new PacketDefinitions.C2S.PingLoadInfoRequest(data);
            return new PingLoadInfoRequest(rq.NetId, rq.Position, rq.UserId, rq.Loaded, rq.Unk2, rq.Ping, rq.Unk3,
                rq.Unk4);
        }

        [PacketType(PacketCmd.PKT_C2S_MOVE_REQ)]
        public static MovementRequest ReadMovementRequest(byte[] data)
        {
            var rq = new PacketDefinitions.C2S.MovementRequest(data);
            MoveType type;
            switch (rq.Type)
            {
                case MovementType.EMOTE:
                    type = MoveType.EMOTE;
                    break;
                case MovementType.MOVE:
                    type = MoveType.MOVE;
                    break;
                case MovementType.ATTACK:
                    type = MoveType.ATTACK;
                    break;
                case MovementType.ATTACKMOVE:
                    type = MoveType.ATTACKMOVE;
                    break;
                case MovementType.STOP:
                    type = MoveType.STOP;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return new MovementRequest(rq.NetIdHeader, type, rq.X, rq.Y, rq.TargetNetId, rq.CoordCount, rq.NetId,
                rq.MoveData);
        }

        [PacketType(PacketCmd.PKT_C2S_QUEST_CLICKED)]
        public static QuestClickedRequest ReadQuestClickedRequest(byte[] data)
        {
            var rq = new QuestClicked(data);
            return new QuestClickedRequest(rq.Netid);
        }

        [PacketType(PacketCmd.PKT_C2S_VIEW_REQ)]
        public static ViewRequest ReadViewRequest(byte[] data)
        {
            var rq = new PacketDefinitions.C2S.ViewRequest(data);
            return new ViewRequest(rq.NetId, rq.X, rq.Zoom, rq.Y, rq.Y2, rq.Width, rq.Height, rq.Unk2, rq.RequestNo);
        }

        [PacketType(PacketCmd.PKT_C2S_SELL_ITEM)]
        public static SellItemRequest ReadSellItemRequest(byte[] data)
        {
            var rq = new SellItem(data);
            return new SellItemRequest(rq.NetId, rq.SlotId);
        }

        [PacketType(PacketCmd.PKT_C2S_SKILL_UP)]
        public static SkillUpRequest ReadSkillUpRequest(byte[] data)
        {
            var rq = new PacketDefinitions.C2S.SkillUpRequest(data);
            return new SkillUpRequest(rq.NetId, rq.Skill);
        }

        [PacketType(PacketCmd.PKT_C2S_USE_OBJECT)]
        public static UseObjectRequest ReadUseObjectRequest(byte[] data)
        {
            var rq = new UseObject(data);
            return new UseObjectRequest(rq.NetId, rq.TargetNetId);
        }

        [PacketType(PacketCmd.PKT_C2S_SYNCH_VERSION)]
        public static SynchVersionRequest ReadSynchVersionRequest(byte[] data)
        {
            var rq = new PacketDefinitions.C2S.SynchVersionRequest(data);
            return new SynchVersionRequest(rq.NetId, rq.Unk1, rq.Version);
        }
    }
}
