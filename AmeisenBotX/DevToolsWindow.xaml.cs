﻿using AmeisenBotX.Core;
using AmeisenBotX.Core.Data.Objects.WowObject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace AmeisenBotX
{
    public partial class DevToolsWindow : Window
    {
        public DevToolsWindow(AmeisenBot ameisenBot)
        {
            AmeisenBot = ameisenBot;

            InitializeComponent();
        }

        public AmeisenBot AmeisenBot { get; private set; }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            RefreshActiveData();
        }

        private void RefreshActiveData()
        {
            if (tabcontrolMain.SelectedIndex == 0)
            {
                listviewCacheNames.Items.Clear();

                foreach (KeyValuePair<ulong, string> x in AmeisenBot.WowInterface.BotCache.NameCache.OrderBy(e => e.Value))
                {
                    listviewCacheNames.Items.Add(x);
                }
            }
            else if (tabcontrolMain.SelectedIndex == 1)
            {
                listviewCacheReactions.Items.Clear();

                foreach (KeyValuePair<(int, int), WowUnitReaction> x in AmeisenBot.WowInterface.BotCache.ReactionCache.OrderBy(e => e.Key.Item1).ThenBy(e => e.Key.Item2))
                {
                    listviewCacheReactions.Items.Add(x);
                }
            }
            else if (tabcontrolMain.SelectedIndex == 2)
            {
                listviewCacheSpellnames.Items.Clear();

                foreach (KeyValuePair<int, string> x in AmeisenBot.WowInterface.BotCache.SpellNameCache.OrderBy(e => e.Value))
                {
                    listviewCacheSpellnames.Items.Add(x);
                }
            }
            else if (tabcontrolMain.SelectedIndex == 3)
            {
                listviewNearWowObjects.Items.Clear();

                List<(WowObject, double)> wowObjects = new List<(WowObject, double)>();

                foreach (WowObject x in AmeisenBot.WowInterface.ObjectManager.WowObjects)
                {
                    wowObjects.Add((x, Math.Round(x.Position.GetDistance(AmeisenBot.WowInterface.ObjectManager.Player.Position), 2)));
                }

                foreach ((WowObject, double) x in wowObjects.OrderBy(e => e.Item2))
                {
                    listviewNearWowObjects.Items.Add(x);
                }
            }
        }

        private void TabcontrolMain_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            RefreshActiveData();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}