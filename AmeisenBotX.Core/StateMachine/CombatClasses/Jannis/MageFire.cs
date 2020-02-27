﻿using AmeisenBotX.Core.Character;
using AmeisenBotX.Core.Character.Comparators;
using AmeisenBotX.Core.Data;
using AmeisenBotX.Core.Data.Enums;
using AmeisenBotX.Core.Data.Objects.WowObject;
using AmeisenBotX.Core.Hook;
using AmeisenBotX.Core.StateMachine.Enums;
using System.Collections.Generic;
using static AmeisenBotX.Core.StateMachine.Utils.AuraManager;
using static AmeisenBotX.Core.StateMachine.Utils.InterruptManager;

namespace AmeisenBotX.Core.StateMachine.CombatClasses.Jannis
{
    public class MageFire : BasicCombatClass
    {
        // author: Jannis Höschele

        private readonly string arcaneIntellectSpell = "Arcane Intellect";
        private readonly string counterspellSpell = "Counterspell";
        private readonly string evocationSpell = "Evocation";
        private readonly string fireballSpell = "Fireball";
        private readonly string hotstreakSpell = "Hot Streak";
        private readonly string iceBlockSpell = "Ice Block";
        private readonly string livingBombSpell = "Living Bomb";
        private readonly string manaShieldSpell = "Mana Shield";
        private readonly string mirrorImageSpell = "Mirror Image";
        private readonly string moltenArmorSpell = "Molten Armor";
        private readonly string pyroblastSpell = "Pyroblast";
        private readonly string scorchSpell = "Scorch";
        private readonly string spellStealSpell = "Spellsteal";

        public MageFire(ObjectManager objectManager, CharacterManager characterManager, HookManager hookManager) : base(objectManager, characterManager, hookManager)
        {
            MyAuraManager.BuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { arcaneIntellectSpell, () =>
                    {
                        HookManager.TargetGuid(ObjectManager.PlayerGuid);
                        return CastSpellIfPossible(arcaneIntellectSpell, true);
                    }
                },
                { moltenArmorSpell, () => CastSpellIfPossible(moltenArmorSpell, true) },
                { manaShieldSpell, () => CastSpellIfPossible(manaShieldSpell, true) }
            };

            TargetAuraManager.DebuffsToKeepActive = new Dictionary<string, CastFunction>()
            {
                { scorchSpell, () => CastSpellIfPossible(scorchSpell, true) },
                { livingBombSpell, () => CastSpellIfPossible(livingBombSpell, true) }
            };

            TargetAuraManager.DispellBuffs = () => HookManager.HasUnitStealableBuffs(WowLuaUnit.Target) && CastSpellIfPossible(spellStealSpell, true);

            TargetInterruptManager.InterruptSpells = new SortedList<int, CastInterruptFunction>()
            {
                { 0, () => CastSpellIfPossible(counterspellSpell, true) }
            };
        }

        public override string Author => "Jannis";

        public override WowClass Class => WowClass.Mage;

        public override Dictionary<string, dynamic> Configureables { get; set; } = new Dictionary<string, dynamic>();

        public override string Description => "FCFS based CombatClass for the Fire Mage spec.";

        public override string Displayname => "Mage Fire";

        public override bool HandlesMovement => false;

        public override bool HandlesTargetSelection => false;

        public override bool IsMelee => false;

        public override IWowItemComparator ItemComparator { get; set; } = new BasicIntellectComparator();

        public override CombatClassRole Role => CombatClassRole.Dps;

        public override string Version => "1.0";

        public override void Execute()
        {
            // we dont want to do anything if we are casting something...
            if (ObjectManager.Player.IsCasting)
            {
                return;
            }

            if (MyAuraManager.Tick()
                || TargetAuraManager.Tick()
                || TargetInterruptManager.Tick())
            {
                return;
            }

            if (ObjectManager.Target != null)
            {
                if (CastSpellIfPossible(mirrorImageSpell, true)
                    || (ObjectManager.Player.HealthPercentage < 16
                        && CastSpellIfPossible(iceBlockSpell, true))
                    || (MyAuraManager.Buffs.Contains(hotstreakSpell.ToLower()) && CastSpellIfPossible(pyroblastSpell, true))
                    || (ObjectManager.Player.ManaPercentage < 40
                        && CastSpellIfPossible(evocationSpell, true))
                    || CastSpellIfPossible(fireballSpell, true))
                {
                    return;
                }
            }
        }

        public override void OutOfCombatExecute()
        {
            if (MyAuraManager.Tick())
            {
                return;
            }
        }
    }
}