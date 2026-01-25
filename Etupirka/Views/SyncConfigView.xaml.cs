using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Etupirka.Properties;
using Etupirka.Sync;

namespace Etupirka.Views
{
    public partial class SyncConfigView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private SyncManager _syncManager;
        private Action _syncNowCallback;

        public SyncConfigView()
        {
            InitializeComponent();
            DataContext = this;
            secretKeyBox.Password = Settings.Default.syncSecretKey;
            secretKeyBox.PasswordChanged += SecretKeyBox_PasswordChanged;
        }

        public void Initialize(SyncManager syncManager, Action syncNowCallback)
        {
            _syncManager = syncManager;
            _syncNowCallback = syncNowCallback;
            MachineId = syncManager.MachineId;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MachineId)));
        }

        public bool SyncEnabled
        {
            get => Settings.Default.syncEnabled;
            set
            {
                Settings.Default.syncEnabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SyncEnabled)));
            }
        }

        public string SyncEndpoint
        {
            get => Settings.Default.syncEndpoint;
            set => Settings.Default.syncEndpoint = value;
        }

        public string SyncAccessKey
        {
            get => Settings.Default.syncAccessKey;
            set => Settings.Default.syncAccessKey = value;
        }

        public string SyncBucketName
        {
            get => Settings.Default.syncBucketName;
            set => Settings.Default.syncBucketName = value;
        }

        public int SyncInterval
        {
            get => Settings.Default.syncInterval;
            set => Settings.Default.syncInterval = value;
        }

        public string MachineId { get; private set; }

        private void SecretKeyBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            Settings.Default.syncSecretKey = secretKeyBox.Password;
        }

        private async void btnTestConnection_Click(object sender, RoutedEventArgs e)
        {
            btnTestConnection.IsEnabled = false;
            lblConnectionStatus.Content = "接続中...";

            try
            {
                _syncManager.Configure(SyncEndpoint, SyncAccessKey, secretKeyBox.Password, SyncBucketName);
                var result = await _syncManager.TestConnectionAsync();

                if (result)
                {
                    lblConnectionStatus.Content = "接続成功";
                    lblConnectionStatus.Foreground = System.Windows.Media.Brushes.Green;
                }
                else
                {
                    lblConnectionStatus.Content = "接続失敗";
                    lblConnectionStatus.Foreground = System.Windows.Media.Brushes.Red;
                }
            }
            catch (Exception ex)
            {
                lblConnectionStatus.Content = $"エラー: {ex.Message}";
                lblConnectionStatus.Foreground = System.Windows.Media.Brushes.Red;
            }

            btnTestConnection.IsEnabled = true;
        }

        private void btnSyncNow_Click(object sender, RoutedEventArgs e)
        {
            _syncNowCallback?.Invoke();
            txtLastSync.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public void UpdateLastSyncTime(DateTime? time)
        {
            if (time.HasValue)
            {
                txtLastSync.Text = time.Value.ToString("yyyy-MM-dd HH:mm:ss");
            }
        }
    }
}
