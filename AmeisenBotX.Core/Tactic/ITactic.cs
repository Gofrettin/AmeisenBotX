﻿using AmeisenBotX.Core.Statemachine.Enums;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Tactic
{
    public interface ITactic
    {
        Dictionary<string, dynamic> Configureables { get; }

        bool ExecuteTactic(CombatClassRole role, bool isMelee, out bool preventMovement, out bool allowAttacking);
    }
}