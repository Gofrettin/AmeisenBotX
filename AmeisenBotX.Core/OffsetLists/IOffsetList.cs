﻿using System;

namespace AmeisenBotX.Core.OffsetLists
{
    public interface IOffsetList
    {
        IntPtr AccountName { get; }

        IntPtr AutolootEnabled { get; }

        IntPtr AutolootPointer { get; }

        IntPtr BattlegroundStatus { get; }

        IntPtr BattlegroundFinished { get; }

        IntPtr CharacterSlotSelected { get; }

        IntPtr ChatBuffer { get; }

        IntPtr ChatNextMessage { get; }

        IntPtr ChatOpened { get; }

        IntPtr ClickToMoveAction { get; }

        IntPtr ClickToMoveDistance { get; }

        IntPtr ClickToMoveEnabled { get; }

        IntPtr ClickToMoveGuid { get; }

        IntPtr ClickToMovePendingMovement { get; }

        IntPtr ClickToMovePointer { get; }

        IntPtr ClickToMoveTurnSpeed { get; }

        IntPtr ClickToMoveX { get; }

        IntPtr ClickToMoveY { get; }

        IntPtr ClickToMoveZ { get; }

        IntPtr ClientConnection { get; }

        IntPtr ComboPoints { get; }

        IntPtr ContinentName { get; }

        IntPtr CorpsePosition { get; }

        IntPtr CurrentlyCastingSpellId { get; }

        IntPtr CurrentlyChannelingSpellId { get; }

        IntPtr CurrentObjectManager { get; }

        IntPtr CvarMaxFps { get; }

        IntPtr DescriptorCombatReach { get; }

        IntPtr DescriptorEnergy { get; }

        IntPtr DescriptorExp { get; }

        IntPtr DescriptorFactionTemplate { get; }

        IntPtr DescriptorHealth { get; }

        IntPtr DescriptorInfoFlags { get; }

        IntPtr DescriptorLevel { get; }

        IntPtr DescriptorMana { get; }

        IntPtr DescriptorMaxEnergy { get; }

        IntPtr DescriptorMaxExp { get; }

        IntPtr DescriptorMaxHealth { get; }

        IntPtr DescriptorMaxMana { get; }

        IntPtr DescriptorMaxRage { get; }

        IntPtr DescriptorMaxRuneenergy { get; }

        IntPtr DescriptorNpcFlags { get; }

        IntPtr DescriptorRage { get; }

        IntPtr DescriptorRuneenergy { get; }

        IntPtr DescriptorTargetGuid { get; }

        IntPtr DescriptorUnitFlags { get; }

        IntPtr DescriptorUnitFlagsDynamic { get; }

        IntPtr EndSceneOffset { get; }

        IntPtr EndSceneOffsetDevice { get; }

        IntPtr EndSceneStaticDevice { get; }

        IntPtr ErrorMessage { get; }

        IntPtr FirstObject { get; }

        IntPtr FunctionCastSpellById { get; }

        IntPtr FunctionClickToMove { get; }

        IntPtr FunctionGetActivePlayerObject { get; }

        IntPtr FunctionGetLocalizedText { get; }

        IntPtr FunctionGetUnitReaction { get; }

        IntPtr FunctionHandleTerrainClick { get; }

        IntPtr FunctionIsClickMoving { get; }

        IntPtr FunctionLuaDoString { get; }

        IntPtr FunctionRenderWorld { get; }

        IntPtr FunctionSendMovementPacket { get; }

        IntPtr FunctionSetFacing { get; }

        IntPtr FunctionSetTarget { get; }

        IntPtr FunctionStopClickToMove { get; }

        IntPtr GameState { get; }

        IntPtr IsAutoAttacking { get; }

        IntPtr IsWorldLoaded { get; }

        IntPtr LastTargetGuid { get; }

        IntPtr LootWindowOpen { get; }

        IntPtr MapId { get; }

        IntPtr NameBase { get; }

        IntPtr NameMask { get; }

        IntPtr NameStore { get; }

        IntPtr NameString { get; }

        IntPtr NextObject { get; }

        IntPtr PartyLeader { get; }

        IntPtr PartyPlayer1 { get; }

        IntPtr PartyPlayer2 { get; }

        IntPtr PartyPlayer3 { get; }

        IntPtr PartyPlayer4 { get; }

        IntPtr PetGuid { get; }

        IntPtr PlayerBase { get; }

        IntPtr PlayerGuid { get; }

        IntPtr PlayerName { get; }

        IntPtr PlayerRotation { get; }

        IntPtr RaidGroupPlayer { get; }

        IntPtr RaidGroupStart { get; }

        IntPtr RaidLeader { get; }

        IntPtr RealmName { get; }

        IntPtr RuneCooldown { get; }

        IntPtr Runes { get; }

        IntPtr RuneType { get; }

        IntPtr TargetGuid { get; }

        IntPtr TickCount { get; }

        IntPtr WowBuild { get; }

        IntPtr WowDynobjectCasterGuid { get; }

        IntPtr WowDynobjectFacing { get; }

        IntPtr WowDynobjectPosition { get; }

        IntPtr WowDynobjectRadius { get; }

        IntPtr WowDynobjectSpellId { get; }

        IntPtr WowGameobjectDisplayId { get; }

        IntPtr WowGameobjectLevel { get; }

        IntPtr WowGameobjectPosition { get; }

        IntPtr WowGameobjectType { get; }

        IntPtr WowObjectDescriptor { get; }

        IntPtr WowObjectGuid { get; }

        IntPtr WowObjectEntryId { get; }

        IntPtr WowObjectScale { get; }

        IntPtr WowObjectPosition { get; }

        IntPtr WowObjectType { get; }

        IntPtr WowUnitPosition { get; }

        IntPtr ZoneId { get; }
    }
}
