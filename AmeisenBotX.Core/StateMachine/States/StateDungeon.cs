﻿using AmeisenBotX.Core.Common;

namespace AmeisenBotX.Core.Statemachine.States
{
    public class StateDungeon : BasicState
    {
        public StateDungeon(AmeisenBotStateMachine stateMachine, AmeisenBotConfig config, WowInterface wowInterface) : base(stateMachine, config, wowInterface)
        {
        }

        public override void Enter()
        {
            WowInterface.MovementEngine.Reset();
            WowInterface.DungeonEngine.Enter();
            StateMachine.OnStateOverride += StateMachine_OnStateOverride;
        }

        public override void Execute()
        {
            if (!WowInterface.ObjectManager.MapId.IsDungeonMap())
            {
                StateMachine.SetState(BotState.Idle);
                return;
            }

            WowInterface.DungeonEngine.Execute();
            WowInterface.CombatClass?.OutOfCombatExecute();
        }

        public override void Leave()
        {
            StateMachine.OnStateOverride -= StateMachine_OnStateOverride;
            WowInterface.MovementEngine.Reset();
            WowInterface.DungeonEngine.Exit();
        }

        private void StateMachine_OnStateOverride(BotState botState)
        {
            if (botState == BotState.Dead)
            {
                WowInterface.DungeonEngine.OnDeath();
            }
        }
    }
}