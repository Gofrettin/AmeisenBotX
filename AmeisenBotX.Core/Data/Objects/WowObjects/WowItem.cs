﻿using AmeisenBotX.Core.Data.Objects.WowObjects.Structs;
using AmeisenBotX.Core.Data.Objects.WowObjects.Structs.Enums;
using AmeisenBotX.Core.Data.Objects.WowObjects.Structs.SubStructs;
using System;
using System.Collections.Generic;

namespace AmeisenBotX.Core.Data.Objects.WowObjects
{
    public class WowItem : WowObject
    {
        public WowItem(IntPtr baseAddress, WowObjectType type, IntPtr descriptorAddress) : base(baseAddress, type, descriptorAddress)
        {
        }

        public int Count { get; set; }

        public List<ItemEnchantment> ItemEnchantments { get; private set; }

        public ulong Owner { get; set; }

        public List<string> GetEnchantmentStrings()
        {
            List<string> enchantments = new List<string>();

            for (int i = 0; i < ItemEnchantments.Count; ++i)
            {
                if (WowEnchantmentHelper.TryLookupEnchantment(ItemEnchantments[i].Id, out string text))
                {
                    enchantments.Add(text);
                }
            }

            return enchantments;
        }

        public override string ToString()
        {
            return $"Item: [{Guid}] ({EntryId}) Owner: {Owner} Count: {Count}";
        }

        public override unsafe void Update()
        {
            base.Update();

            if (WowInterface.I.XMemory.ReadStruct(DescriptorAddress + RawWowObject.EndOffset, out RawWowItem objPtr))
            {
                Count = objPtr.StackCount;
                Owner = objPtr.Owner;

                ItemEnchantments = new List<ItemEnchantment>()
                {
                    objPtr.Enchantment1,
                    objPtr.Enchantment2,
                    objPtr.Enchantment3,
                    objPtr.Enchantment4,
                    objPtr.Enchantment5,
                    objPtr.Enchantment6,
                    objPtr.Enchantment7,
                    objPtr.Enchantment8,
                    objPtr.Enchantment9,
                    objPtr.Enchantment10,
                    objPtr.Enchantment11,
                    objPtr.Enchantment12,
                };
            }
        }
    }
}