using GameServerCore.Packets.PacketDefinitions.Requests;

namespace GameServerCore.Packets.Interfaces
{
    public interface IPacketReader
    {
        SwapItemsRequest ReadSwapItemsRequest(byte[] data);
        AttentionPingRequest ReadAttentionPingRequest(byte[] data);
        AutoAttackOptionRequest ReadAutoAttackOptionRequest(byte[] data);
        BlueTipClickedRequest ReadBlueTipRequest(byte[] data);
        BuyItemRequest ReadBuyItemRequest(byte[] data);
        CastSpellRequest ReadCastSpellRequest(byte[] data);
        ChatMessageRequest ReadChatMessageRequest(byte[] data);
        ClickRequest ReadClickRequest(byte[] data);
        CursorPositionOnWorldRequest ReadCursorPositionOnWorldRequest(byte[] data);
        EmotionPacketRequest ReadEmotionPacketRequest(byte[] data);
        HeartbeatRequest ReadHeartbeatRequest(byte[] data);
        PingLoadInfoRequest ReadPingLoadInfoRequest(byte[] data);
        MovementRequest ReadMovementRequest(byte[] data);
        QuestClickedRequest ReadQuestClickedRequest(byte[] data);
        ViewRequest ReadViewRequest(byte[] data);
        SellItemRequest ReadSellItemRequest(byte[] data);
        SkillUpRequest ReadSkillUpRequest(byte[] data);
        UseObjectRequest ReadUseObjectRequest(byte[] data);
        SynchVersionRequest ReadSynchVersionRequest(byte[] data);
    }
}
