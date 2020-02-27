﻿using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.StateMachine.Enums;
using AmeisenBotX.Core.StateMachine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using static AmeisenBotX.Core.StateMachine.Utils.AuraManager;

namespace AmeisenBotX.Core.StateMachine.CombatClasses.Jannis
{
    public class PriestDiscipline : BasicCombatClass
    {
        // author: Jannis Höschele

        private readonly string bindingHealSpell = "Binding Heal";
        private readonly int deadPartymembersCheckTime = 4;
        private readonly string desperatePrayerSpell = "Desperate Prayer";
        private readonly string flashHealSpell = "Flash Heal";
        private readonly string greaterHealSpell = "Greater Heal";
        private readonly string hymnOfHopeSpell = "Hymn of Hope";
        private readonly string innerFireSpell = "Inner Fire";
        private readonly string penanceSpell = "Penance";
        private readonly string powerWordFortitudeSpell = "Power Word: Fortitude";
        private readonly string powerWordShieldSpell = "Power Word: Shield";
        private readonly string prayerOfHealingSpell = "Prayer of Healing";
        private readonly string prayerOfMendingSpell = "Prayer of Mending";
        private readonly string renewSpell = "Renew";
        private readonly string resurrectionSpell = "Resurrection";
        private readonly string weakenedSoulSpell = "Weakened Soul";

        public PriestDiscipline(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager) : base(objectManager, characterManager, hookManager)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { powerWordFortitudeSpell, () =>
                    {
                        HookManager.TargetGuid(ObjectManager.PlayerGuid);
                        return CastSpellIfPossible(powerWordFortitudeSpell, true);
                    }
                },
                { innerFireSpell, () => CastSpellIfPossible(innerFireSpell, true) }
            };

            SpellUsageHealDict = new Dictionary<int, string>()
            {
                { 0, flashHealSpell },
                { 3000, penanceSpell },
                { 5000, greaterHealSpell },
            };
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Priest;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Discipline Priest spec.";

        public override string Displayname => "Priest Discipline";

        public override bool HandlesMovement => false;

        public override bool HandlesTargetSelection => true;

        public override bool IsMelee => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicSpiritComparator();

        public override CombatClassRole Role => CombatClassRole.Heal;

        public override string Version => "1.0";

        private DateTime LastDeadPartymembersCheck { get; set; }

        private Dictionary<int, string> SpellUsageHealDict { get; }

        public override void Execute()
        {
            // we dont want to do anything if we are casting something...
            if (ObjectManager.Player.IsCasting)
            {
                return;
            }

            if (NeedToHealSomeone(out List<WowPlayer> playersThatNeedHealing))
            {
                HandleTargetSelection(playersThatNeedHealing);
                ObjectManager.UpdateObject(ObjectManager.Player.Type, ObjectManager.Player.BaseAddress);

                WowUnit target = ObjectManager.Target;
                if (target != null)
                {
                    ObjectManager.UpdateObject(target.Type, target.BaseAddress);

                    if (playersThatNeedHealing.Count > 4
                        && CastSpellIfPossible(prayerOfHealingSpell, true))
                    {
                        return;
                    }

                    if (target.Guid != ObjectManager.PlayerGuid
                        && target.HealthPercentage < 70
                        && ObjectManager.Player.HealthPercentage < 70
                        && CastSpellIfPossible(bindingHealSpell, true))
                    {
                        return;
                    }

                    if (ObjectManager.Player.ManaPercentage < 50
                        && CastSpellIfPossible(hymnOfHopeSpell))
                    {
                        return;
                    }

                    if (ObjectManager.Player.HealthPercentage < 20
                        && CastSpellIfPossible(desperatePrayerSpell))
                    {
                        return;
                    }

                    List<string> targetBuffs = HookManager.GetAuras(WowLuaUnit.Target);

                    if ((target.HealthPercentage < 85
                            && !targetBuffs.Any(e => e.Equals(weakenedSoulSpell, StringComparison.OrdinalIgnoreCase))
                            && !targetBuffs.Any(e => e.Equals(powerWordShieldSpell, StringComparison.OrdinalIgnoreCase))
                            && CastSpellIfPossible(powerWordShieldSpell, true))
                        || (target.HealthPercentage < 80
                            && !targetBuffs.Any(e => e.Equals(renewSpell, StringComparison.OrdinalIgnoreCase))
                            && CastSpellIfPossible(renewSpell, true)))
                    {
                        return;
                    }

                    double healthDifference = target.MaxHealth - target.Health;
                    List<KeyValuePair<int, string>> spellsToTry = SpellUsageHealDict.Where(e => e.Key <= healthDifference).ToList();

                    foreach (KeyValuePair<int, string> keyValuePair in spellsToTry.OrderByDescending(e => e.Value))
                    {
                        if (CastSpellIfPossible(keyValuePair.Value, true))
                        {
                            return;
                        }
                    }
                }
                else
                {
                    if (MyAuraManager.Tick())
                    {
                        return;
                    }
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            if (MyAuraManager.Tick()
                || (DateTime.Now - LastDeadPartymembersCheck > TimeSpan.FromSeconds(deadPartymembersCheckTime)
                && HandleDeadPartymembers()))
            {
                return;
            }
        }

        private bool HandleDeadPartymembers()
        {
            if (!Spells.ContainsKey(resurrectionSpell))
            {
                Spells.Add(resurrectionSpell, CharacterManager.SpellBook.GetSpellByName(resurrectionSpell));
            }

            if (Spells[resurrectionSpell] != null
                && !CooldownManager.IsSpellOnCooldown(resurrectionSpell)
                && Spells[resurrectionSpell].Costs < ObjectManager.Player.Mana)
            {
                IEnumerable<WowPlayer> players = ObjectManager.WowObjects.OfType<WowPlayer>();
                List<WowPlayer> groupPlayers = players.Where(e => e.IsDead && e.Health == 0 && ObjectManager.PartymemberGuids.Contains(e.Guid)).ToList();

                if (groupPlayers.Count > 0)
                {
                    HookManager.TargetGuid(groupPlayers.First().Guid);
                    HookManager.CastSpell(resurrectionSpell);
                    CooldownManager.SetSpellCooldown(resurrectionSpell, (int)HookManager.GetSpellCooldown(resurrectionSpell));
                    return true;
                }
            }

            return false;
        }

        private void HandleTargetSelection(List<WowPlayer> possibleTargets)
        {
            // select the one with lowest hp
            HookManager.TargetGuid(possibleTargets.OrderBy(e => e.HealthPercentage).First().Guid);
        }

        private bool NeedToHealSomeone(out List<WowPlayer> playersThatNeedHealing)
        {
            IEnumerable<WowPlayer> players = ObjectManager.WowObjects.OfType<WowPlayer>();
            List<WowPlayer> groupPlayers = players.Where(e => !e.IsDead && e.Health > 1 && ObjectManager.PartymemberGuids.Contains(e.Guid) && e.Position.GetDistance2D(ObjectManager.Player.Position) < 35).ToList();

            groupPlayers.Add(ObjectManager.Player);

            playersThatNeedHealing = groupPlayers.Where(e => e.HealthPercentage < 90).ToList();

            return playersThatNeedHealing.Count > 0;
        }
    }
}