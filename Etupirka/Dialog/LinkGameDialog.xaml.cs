using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Etupirka.Sync;
using MahApps.Metro.Controls;

namespace Etupirka.Dialog
{
    public partial class LinkGameDialog : MetroWindow
    {
        public class CloudGameItem
        {
            public string LinkId { get; set; }
            public string Title { get; set; }
        }

        private readonly GameExecutionInfo _localGame;
        private readonly SyncManager _syncManager;
        private readonly GameLinksData _linksData;

        public int? SelectedEsid { get; private set; }
        public string SelectedLinkId { get; private set; }
        public bool CreateNewLink { get; private set; }

        public LinkGameDialog(GameExecutionInfo localGame, SyncManager syncManager, GameLinksData linksData)
        {
            InitializeComponent();
            _localGame = localGame;
            _syncManager = syncManager;
            _linksData = linksData;

            if (Properties.Settings.Default.disableGlowBrush)
            {
                this.GlowBrush = null;
            }

            txtLocalGame.Text = localGame.Title;

            // Populate cloud games that don't have this machine linked
            var unlinkedGames = linksData.Links.Values
                .Where(l => !l.Machines.ContainsKey(syncManager.MachineId))
                .Select(l => new CloudGameItem { LinkId = l.LinkId, Title = l.Title })
                .ToList();

            cmbCloudGames.ItemsSource = unlinkedGames;
            if (unlinkedGames.Any())
            {
                cmbCloudGames.SelectedIndex = 0;
            }
            else
            {
                rbExisting.IsEnabled = false;
                rbNewLink.IsChecked = true;
            }
        }

        private void btnLink_Click(object sender, RoutedEventArgs e)
        {
            if (rbSetEsid.IsChecked == true)
            {
                SelectedEsid = (int)nudEsid.Value;
            }
            else if (rbNewLink.IsChecked == true)
            {
                CreateNewLink = true;
            }
            else if (rbExisting.IsChecked == true && cmbCloudGames.SelectedItem != null)
            {
                SelectedLinkId = ((CloudGameItem)cmbCloudGames.SelectedItem).LinkId;
            }
            else
            {
                MessageBox.Show("リンク方法を選択してください。", "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }
    }
}
