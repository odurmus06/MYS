using MYSConnector.Views;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System;

namespace MYSConnector
{
    public partial class MainWindow : Window
    {
        private AtoListView _atoView;
        private AcoListView _acoView;
        private SettingsView _settingsView;
        private static readonly TimeSpan SessionTimeout = TimeSpan.FromMinutes(30);
        private readonly DispatcherTimer _sessionTimer;
        private DateTime _lastActivity;

        public MainWindow()
        {
            InitializeComponent();

            AppSettings.Load();

            _atoView = new AtoListView();
            _acoView = new AcoListView();
            _settingsView = new SettingsView();

            _sessionTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            _sessionTimer.Tick += SessionTimer_Tick;
            PreviewMouseDown += MainWindow_MouseActivity;
            PreviewKeyDown += MainWindow_KeyActivity;

            // Give views a way to navigate to settings
            _atoView.Tag = this;
            _acoView.Tag = this;

            MainContentFrame.Content = _atoView;
            PageTitle.Text = "ATO — Air Tasking Order";
            SetActiveNavigation(NavAtoButton);
            SetBottomContext("ATO MESSAGES");

            Loaded += (s, e) => LoginUsername.Focus();
        }

        public void NavigateToSettings()
        {
            MainContentFrame.Content = _settingsView;
            PageTitle.Text = "Settings";
            SetActiveNavigation(NavSettingsButton);
            SetBottomContext("SETTINGS");
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string user = LoginUsername.Text != null ? LoginUsername.Text.Trim() : "";
            string pass = LoginPassword.Password != null ? LoginPassword.Password.Trim() : "";

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                ShowLoginError("Please enter username and password.");
                return;
            }

            LoginOverlay.Visibility = Visibility.Collapsed;
            AppContent.IsEnabled = true;
            _lastActivity = DateTime.Now;
            _sessionTimer.Start();
            UpdateBottomStatus("READY", "ATO MESSAGES", "Ready");
        }

        private void LoginField_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                LoginButton_Click(sender, new RoutedEventArgs());
            }
        }

        private void ShowLoginError(string msg)
        {
            LoginError.Text = msg;
            LoginError.Visibility = Visibility.Visible;
        }

        private void NavAto_Click(object sender, RoutedEventArgs e)
        {
            MainContentFrame.Content = _atoView;
            PageTitle.Text = "ATO — Air Tasking Order";
            SetActiveNavigation(NavAtoButton);
            SetBottomContext("ATO MESSAGES");
        }

        private void NavAco_Click(object sender, RoutedEventArgs e)
        {
            MainContentFrame.Content = _acoView;
            PageTitle.Text = "ACO — Airspace Control Order";
            SetActiveNavigation(NavAcoButton);
            SetBottomContext("ACO MESSAGES");
        }

        private void NavSettings_Click(object sender, RoutedEventArgs e)
        {
            NavigateToSettings();
        }

        private void SetActiveNavigation(Button activeButton)
        {
            NavAtoButton.Tag = null;
            NavAcoButton.Tag = null;
            NavSettingsButton.Tag = null;
            if (activeButton != null)
                activeButton.Tag = "Active";
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            _sessionTimer.Stop();
            _atoView.ResetFilters();
            _acoView.ResetFilters();
            LoginUsername.Text = "";
            LoginPassword.Password = "";
            LoginError.Visibility = Visibility.Collapsed;
            LoginOverlay.Visibility = Visibility.Visible;
            AppContent.IsEnabled = false;
            UpdateBottomStatus("READY", "SIGNED OUT", "Signed out");
            LoginUsername.Focus();
        }

        public void UpdateBottomStatus(string serviceStatus, string context, string operation)
        {
            BottomServiceStatusText.Text = "SERVICE STATUS: " + serviceStatus;
            BottomServiceStatusText.Foreground = (System.Windows.Media.Brush)FindResource(
                serviceStatus == "ERROR" ? "ThemeAccentRedBrush" :
                serviceStatus == "BUSY" ? "ThemeAccentPrimaryBrush" : "ThemeAccentGreenBrush");
            BottomContextText.Text = context ?? string.Empty;
            BottomOperationText.Text = operation ?? string.Empty;
        }

        public bool IsCurrentView(object view)
        {
            return MainContentFrame.Content == view;
        }

        private void SetBottomContext(string context)
        {
            BottomContextText.Text = context;
            BottomOperationText.Text = "Ready";
            BottomServiceStatusText.Text = "SERVICE STATUS: READY";
            BottomServiceStatusText.Foreground = (System.Windows.Media.Brush)FindResource("ThemeAccentGreenBrush");
        }

        private void MainWindow_MouseActivity(object sender, MouseButtonEventArgs e)
        {
            if (LoginOverlay.Visibility == Visibility.Collapsed)
                _lastActivity = DateTime.Now;
        }

        private void MainWindow_KeyActivity(object sender, KeyEventArgs e)
        {
            if (LoginOverlay.Visibility == Visibility.Collapsed)
                _lastActivity = DateTime.Now;
        }

        private void SessionTimer_Tick(object sender, EventArgs e)
        {
            if (LoginOverlay.Visibility != Visibility.Collapsed)
                return;

            if (DateTime.Now - _lastActivity >= SessionTimeout)
            {
                Logout_Click(this, new RoutedEventArgs());
                ShowLoginError("Session expired. Please sign in again.");
            }
        }
    }
}
