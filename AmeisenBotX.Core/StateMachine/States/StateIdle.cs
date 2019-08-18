﻿using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmeisenBotX.Core.StateMachine.States
{
    public class StateIdle : State
    {
        private AmeisenBotConfig Config { get; }
        private ObjectManager ObjectManager { get; }
        private HookManager HookManager { get; }

        private bool NeedToSetupHook { get; set; }

        public StateIdle(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, ObjectManager objectManager, HookManager hookManager) : base(stateMachine)
        {
            Config = config;
            ObjectManager = objectManager;
            HookManager = hookManager;
            NeedToSetupHook = true;
        }

        public override void Enter()
        {
            if (NeedToSetupHook)
                if (HookManager.SetupEndsceneHook())
                    NeedToSetupHook = false;
        }

        public override void Execute()
        {            
            if (AmeisenBotStateMachine.XMemory.Process.HasExited)
                AmeisenBotStateMachine.SetState(AmeisenBotState.None);

            if (IsUnitToFollowThere())
                AmeisenBotStateMachine.SetState(AmeisenBotState.Following);
        }

        public bool IsUnitToFollowThere()
        {
            WowPlayer PlayerToFollow = null;

            // TODO: make this crap less redundant
            // check the specific character
            if (Config.FollowSpecificCharacter)
            {
                PlayerToFollow = ObjectManager.WowObjects.OfType<WowPlayer>().FirstOrDefault(p => p.Name == Config.SpecificCharacterToFollow);
                PlayerToFollow = SkipIfOutOfRange(PlayerToFollow);

            }

            // check the group/raid leader
            if (PlayerToFollow == null && Config.FollowGroupLeader)
            {
                PlayerToFollow = ObjectManager.WowObjects.OfType<WowPlayer>().FirstOrDefault(p => p.Guid == ObjectManager.PartyleaderGuid);
                PlayerToFollow = SkipIfOutOfRange(PlayerToFollow);
            }

            // check the group members
            if (PlayerToFollow == null && Config.FollowGroupMembers)
            {
                PlayerToFollow = ObjectManager.WowObjects.OfType<WowPlayer>().FirstOrDefault(p => ObjectManager.PartymemberGuids.Contains(p.Guid));
                PlayerToFollow = SkipIfOutOfRange(PlayerToFollow);
            }

            return PlayerToFollow != null;
        }

        private WowPlayer SkipIfOutOfRange(WowPlayer PlayerToFollow)
        {
            if (PlayerToFollow != null)
            {
                double distance = PlayerToFollow.Position.GetDistance(ObjectManager.Player.Position);
                if (UnitIsOutOfRange(distance))
                    PlayerToFollow = null;
            }

            return PlayerToFollow;
        }

        private bool UnitIsOutOfRange(double distance)
           => (distance < Config.MinFollowDistance || distance > Config.MaxFollowDistance);

        public override void Exit()
        {

        }
    }
}
