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
    public class ShamanRestoration : BasicCombatClass
    {
        // author: Jannis Höschele

        private readonly string ancestralSpiritSpell = "Ancestral Spirit";
        private readonly int buffCheckTime = 8;
        private readonly string chainHealSpell = "Chain Heal";
        private readonly int deadPartymembersCheckTime = 4;
        private readonly string earthlivingWeaponSpell = "Earthliving Weapon";
        private readonly string earthShieldSpell = "Earth Shield";
        private readonly string healingWaveSpell = "Healing Wave";
        private readonly string naturesSwiftnessSpell = "Nature's Swiftness";
        private readonly string riptideSpell = "Riptide";
        private readonly string tidalForceSpell = "Tidal Force";
        private readonly string waterShieldSpell = "Water Shield";

        public ShamanRestoration(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager) : base(objectManager, characterManager, hookManager)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { waterShieldSpell, () => CastSpellIfPossible(waterShieldSpell, true) }
            };

            SpellUsageHealDict = new Dictionary<int, string>()
            {
                { 0, riptideSpell },
                { 5000, healingWaveSpell },
            };
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Shaman;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Restoration Shaman spec.";

        public override string Displayname => "Shaman Restoration";

        public override bool HandlesMovement => false;

        public override bool HandlesTargetSelection => true;

        public override bool IsMelee => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicSpiritComparator();

        public override CombatClassRole Role => CombatClassRole.Heal;

        public override string Version => "1.0";

        private DateTime LastBuffCheck { get; set; }

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

                if (ObjectManager.Target != null)
                {
                    if (ObjectManager.Target.HealthPercentage < 25
                        && CastSpellIfPossible(earthShieldSpell, true))
                    {
                        return;
                    }

                    if (playersThatNeedHealing.Count > 4
                        && CastSpellIfPossible(chainHealSpell, true))
                    {
                        return;
                    }

                    if (playersThatNeedHealing.Count > 6
                        && (CastSpellIfPossible(naturesSwiftnessSpell, true)
                        || CastSpellIfPossible(tidalForceSpell, true)))
                    {
                        return;
                    }

                    double healthDifference = ObjectManager.Target.MaxHealth - ObjectManager.Target.Health;
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
            if (!Spells.ContainsKey(ancestralSpiritSpell))
            {
                Spells.Add(ancestralSpiritSpell, CharacterManager.SpellBook.GetSpellByName(ancestralSpiritSpell));
            }

            if (Spells[ancestralSpiritSpell] != null
                && !CooldownManager.IsSpellOnCooldown(ancestralSpiritSpell)
                && Spells[ancestralSpiritSpell].Costs < ObjectManager.Player.Mana)
            {
                IEnumerable<WowPlayer> players = ObjectManager.WowObjects.OfType<WowPlayer>();
                List<WowPlayer> groupPlayers = players.Where(e => e.IsDead && e.Health == 0 && ObjectManager.PartymemberGuids.Contains(e.Guid)).ToList();

                if (groupPlayers.Count > 0)
                {
                    HookManager.TargetGuid(groupPlayers.First().Guid);
                    HookManager.CastSpell(ancestralSpiritSpell);
                    CooldownManager.SetSpellCooldown(ancestralSpiritSpell, (int)HookManager.GetSpellCooldown(ancestralSpiritSpell));
                    return true;
                }
            }

            return false;
        }

        private void HandleTargetSelection(List<WowPlayer> possibleTargets)
        {
            // select the one with lowest hp
            WowUnit target = possibleTargets.Where(e => !e.IsDead && e.Health > 1).OrderBy(e => e.HealthPercentage).First();
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