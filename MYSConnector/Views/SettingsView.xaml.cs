using System.Windows;
using System.Windows.Controls;

namespace MYSConnector.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
            TimeoutTextBox.Text = AppSettings.ServiceTimeoutSeconds.ToString();
            RetriesTextBox.Text = AppSettings.ServiceMaxRetries.ToString();
            RefreshFolderDisplay();
        }

        private void RefreshFolderDisplay()
        {
            if (AppSettings.HasOutputFolder)
            {
                FolderPathText.Text = AppSettings.OutputFolder;
                FolderPathText.Foreground = (System.Windows.Media.Brush)FindResource("ThemeTextPrimaryBrush");
                StatusBorder.Visibility = Visibility.Visible;
                StatusText.Text = "Output folder is configured";
                StatusText.Foreground = (System.Windows.Media.Brush)FindResource("ThemeAccentGreenBrush");
            }
            else
            {
                FolderPathText.Text = "No folder selected...";
                FolderPathText.Foreground = (System.Windows.Media.Brush)FindResource("ThemeTextMutedBrush");
                StatusBorder.Visibility = Visibility.Collapsed;
            }

            if (!string.IsNullOrWhiteSpace(AppSettings.LastError))
            {
                StatusBorder.Visibility = Visibility.Visible;
                StatusText.Text = AppSettings.LastError;
                StatusText.Foreground = (System.Windows.Media.Brush)FindResource("ThemeAccentRedBrush");
            }
        }

        private void BrowseFolder_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            dlg.Description = "Select output folder for parsed messages";
            dlg.ShowNewFolderButton = true;

            if (!string.IsNullOrWhiteSpace(AppSettings.OutputFolder))
                dlg.SelectedPath = AppSettings.OutputFolder;

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string error;
                if (!AppSettings.TrySetOutputFolder(dlg.SelectedPath, out error))
                {
                    MessageBox.Show(error, "Output Folder", MessageBoxButton.OK, MessageBoxImage.Warning);
                    RefreshFolderDisplay();
                    return;
                }
                RefreshFolderDisplay();
            }
        }

        private void SaveServicePolicy_Click(object sender, RoutedEventArgs e)
        {
            int timeoutSeconds;
            int maxRetries;
            if (!int.TryParse(TimeoutTextBox.Text, out timeoutSeconds) ||
                !int.TryParse(RetriesTextBox.Text, out maxRetries))
            {
                MessageBox.Show("Timeout and retry values must be whole numbers.",
                    "Service Policy", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string error;
            if (!AppSettings.TrySetServicePolicy(timeoutSeconds, maxRetries, out error))
            {
                MessageBox.Show(error, "Service Policy", MessageBoxButton.OK, MessageBoxImage.Warning);
                RefreshFolderDisplay();
                return;
            }

            StatusBorder.Visibility = Visibility.Visible;
            StatusText.Text = "Service policy saved";
            StatusText.Foreground = (System.Windows.Media.Brush)FindResource("ThemeAccentGreenBrush");
        }
    }
}
