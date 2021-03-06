﻿using AmeisenBotX.Core.Common;
using AmeisenBotX.Core.Data.Objects.WowObjects;
using AmeisenBotX.Core.Statemachine.Utils.TargetSelectionLogic;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Statemachine.Utils
{
    public class TargetManager
    {
        public TargetManager(BasicTargetSelectionLogic targetSelectionLogic, TimeSpan minTargetSwitchTime)
        {
            TargetSelectionLogic = targetSelectionLogic;

            TargetSwitchEvent = new TimegatedEvent(minTargetSwitchTime);

            PriorityTargets = new List<int>();
        }

        public IEnumerable<int> BlacklistedTargets { get => TargetSelectionLogic.BlacklistedTargets; set => TargetSelectionLogic.BlacklistedTargets = value; }

        public IEnumerable<int> PriorityTargets { get => TargetSelectionLogic.PriorityTargets; set => TargetSelectionLogic.PriorityTargets = value; }

        public BasicTargetSelectionLogic TargetSelectionLogic { get; }

        private TimegatedEvent TargetSwitchEvent { get; set; }

        public bool GetUnitToTarget(out IEnumerable<WowUnit> possibleTargets)
        {
            if (TargetSwitchEvent.Run() && TargetSelectionLogic.SelectTarget(out IEnumerable<WowUnit> possibleTargetsFromLogic))
            {
                possibleTargets = possibleTargetsFromLogic;
                return true;
            }

            possibleTargets = null;
            return false;
        }
    }
}