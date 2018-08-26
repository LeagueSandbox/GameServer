using System;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain;


// FIXME: define these as common classes, should be added with the full integration
using NetID = System.UInt32;
using ClientID = System.UInt32;
using Waypoints = System.UInt32; // wrong, only for compilation
using Vector2D = System.UInt32; // wrong, only for compilation
using Vector3D = System.UInt32; // wrong, only for compilation
using TipCommand = System.UInt32; // wrong, only for compilation
using TipConfig = System.UInt32; // wrong, only for compilation
using ChatType = System.UInt32; // wrong, only for compilation
using PingType = System.UInt32; // wrong, only for compilation
using RespawnPointEvent = System.UInt32; // wrong, only for compilation
using QuestEvent = System.UInt32; // wrong, only for compilation
using VOFlags = System.UInt32; // wrong, only for compilation
using DrawPathNodeType = System.UInt32; // wrong, only for compilation


namespace GameServerCore.Packets.Interfaces
{
    public interface IClientAPI
    {
        void OnSelectObject(ClientID src, NetID selected);
        void OnSpellCast(NetID src, IAttackableUnit target, ISpell spell, Vector2D start, Vector2D end);
        void OnPing(NetID src, PingType type, Vector2D point, NetID target);
        void OnAutoAttack(NetID src, bool activated);
        void OnTipEvent(NetID src, Int32 tipId, TipCommand command);
        void OnBuyItem(NetID src, Int32 itemId);
        void OnChatBoxMsg(NetID src, ChatType type, string msg);
        void OnCursorPositionOnWorld(NetID src, Vector3D pos);
        void OnEmotion(NetID src, Int32 emotionId);
        void OnDisconnect(NetID src);
        void OnHeartBeat(NetID src, float recieveTime, float ackTime);
        void OnKeyCheck(Int32 playerNum, Int64 userId, Int32 versionNum, Byte[] blowfishKey);
        void OnPingLoadInfo(NetID src, Int32 position, Int64 userId, float loaded, Int16 ping);
        void OnLockCamera(NetID src);
        void OnClientReady(NetID src, TipConfig tip);
        void OnMove(NetID src, Waypoints[] way, NetID target);
        void OnQueryStatus(NetID src); // ???
        void OnQuestClick(NetID src, Int32 questId);
        void OnScoreboardOpened(NetID src);
        void OnSellItem(NetID src, Int32 SlotID); // maybe rename to OnRemoveItem(Boolean sold,...); 
        void OnSkillUp(NetID src, Int32 skillId);
        void OnCharacterSelected(NetID src); // used to spawn, why?
        void OnPauseGame(NetID src);
        void OnSwapItem(NetID src, Int32 slot1, Int32 slot2);
        void OnSyncVersion(ClientID src, string version);
        void OnUseObject(NetID src, NetID target);
        void OnSoftReconnect(NetID src);
        void OnTeamSurrenderVote(NetID src, Int32 votedYes);
        void OnUseItem(NetID src, Int32 slot); // not in packets exactly
        void OnShopOpened(NetID src);
        void OnPlayVOCommand(NetID src, NetID target, Int32 CommandId, Int32 eventHash, VOFlags flags);
        void OnStatsUpdate(NetID src); // when and why?
        void OnClientFinished(NetID src); // finish what?
        void OnQuestEvent(NetID src, QuestEvent qevent, Int32 questId);
        void OnRespawnPointEvent(NetID src, RespawnPointEvent revent, Int32 uiElementId);
        void OnSpellChargeUpdate(NetID src, Int32 slot, Vector3D position, Boolean isSummonerSpellBook, Boolean ForceStop);
        void OnSpectatorData(NetID src, Int32 startChunkId, Int32 keyframeId, Boolean sendMetaData, Boolean jumpToLatest);
        void OnSpectatorMetadata(NetID src, String jsonMetadata);
        void OnPlayContextualEmote(NetID src);
        void OnTeamBalanceVote(NetID src, Int32 votedYes);
        void OnUnitSendDrawPath(NetID src, NetID target, Vector3D point, DrawPathNodeType type); // used in tutorial and practice?
        void OnUndoItem(NetID src);
        void OnCheatLogGoldSources(NetID src); // client anticheat?
    }
}
