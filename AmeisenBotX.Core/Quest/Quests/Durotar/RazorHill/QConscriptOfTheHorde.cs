using AmeisenBotX.Core.Movement.Pathfinding.Objects;
using AmeisenBotX.Core.Quest.Objects.Objectives;
using AmeisenBotX.Core.Quest.Objects.Quests;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Quest.Quests.Durotar.RazorHill
{
    class QConscriptOfTheHorde : BotQuest
    {
        public QConscriptOfTheHorde(WowInterface wowInterface)
            : base(wowInterface, 840, "Conscript of the Horde", 10, 1,
                () => (wowInterface.ObjectManager.GetClosestWowUnitByNpcId(new List<int> { 3336 }), new Vector3(271.80f, -4650.83f, 11.79f)),
                () => (wowInterface.ObjectManager.GetClosestWowUnitByNpcId(new List<int> { 3337 }), new Vector3(303.43f, -3686.16f, 27.15f)),
                null)
        {}
    }
}
