using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace MYSConnector.Views
{
    public partial class AtoListView : UserControl
    {
        private List<AtoMessage> _allMessages = new List<AtoMessage>();
        private ObservableCollection<AtoMessage> _filteredMessages = new ObservableCollection<AtoMessage>();
        private CancellationTokenSource _searchCancellation;
        private CancellationTokenSource _parseCancellation;
        private static readonly TimeSpan MinimumLoadingDuration = TimeSpan.FromSeconds(3);
        private bool _serviceError;
        private string _statusDetail = "Ready";

        public AtoListView()
        {
            InitializeComponent();
            LoadDummyData();
            AtoDataGrid.ItemsSource = _filteredMessages;
            Unloaded += AtoListView_Unloaded;
        }

        private void LoadDummyData()
        {
            _allMessages = new List<AtoMessage>
            {
                new AtoMessage
                {
                    MsgId = "ATO-2026-0301", Dtg = "050800ZMAR2026",
                    TaskUnit = "152 SQN / F-16C BLK50", MissionType = "DCA", MissionName = "Anatolian Shield", Callsign = "TIGER11", Status = "Active",
                    RawMessage = "MSGID/ATO/HQ AIR/2026-0301//\nTASKUNIT/152 SQN//\nMSNDAT/050800ZMAR2026/DCA//\nAMSNLOC/KONYA AB//\nCALLSIGN/TIGER11//\nIFF-SIF/MODE3A/2234//\nWEAPONS/AIM120C/4/AIM9X/2//\nFREQ/UHF/352.400//"
                },
                new AtoMessage
                {
                    MsgId = "ATO-2026-0302", Dtg = "051400ZMAR2026",
                    TaskUnit = "113 SQN / F-16D SNIPERS", MissionType = "SEAD", MissionName = "Silent Viper", Callsign = "VIPER21", Status = "Pending",
                    RawMessage = "MSGID/ATO/HQ AIR/2026-0302//\nTASKUNIT/113 SQN//\nMSNDAT/051400ZMAR2026/SEAD//\nAMSNLOC/DIYARBAKIR AB//\nCALLSIGN/VIPER21//\nIFF-SIF/MODE3A/4412//\nWEAPONS/AGM88/4/AIM120C/2//\nTGTDAT/SAM SITE/391500N0432000E//\nFREQ/UHF/281.700//"
                },
                new AtoMessage
                {
                    MsgId = "ATO-2026-0303", Dtg = "060930ZMAR2026",
                    TaskUnit = "161 SQN / F-16C LANTIRN", MissionType = "CAS", MissionName = "Falcon Support", Callsign = "FALCON31", Status = "Parsed",
                    RawMessage = "MSGID/ATO/HQ AIR/2026-0303//\nTASKUNIT/161 SQN//\nMSNDAT/060930ZMAR2026/CAS//\nAMSNLOC/BALIKESIR AB//\nCALLSIGN/FALCON31//\nIFF-SIF/MODE3A/3356//\nWEAPONS/GBU12/4/AIM9X/2//\nTGTDAT/ARMOR COLUMN/394530N0283000E//\nFREQ/VHF/122.100//\nAAR/YES/KC135/TEXACO/AR201/0800-1000Z//"
                },
                new AtoMessage
                {
                    MsgId = "ATO-2026-0304", Dtg = "061245ZMAR2026",
                    TaskUnit = "132 SQN / F-16C", MissionType = "OCA", MissionName = "Northern Hawk", Callsign = "HAWK41", Status = "Error",
                    RawMessage = "MSGID/ATO/HQ AIR/2026-0304//\nTASKUNIT/132 SQN//\nMSNDAT/061245ZMAR2026/OCA//\nAMSNLOC/MERZIFON AB//\nCALLSIGN/HAWK41//\nIFF-SIF/MODE3A/5501//\nWEAPONS/AIM120C/6//\nFREQ/UHF/315.200//"
                },
                new AtoMessage
                {
                    MsgId = "ATO-2026-0305", Dtg = "071000ZMAR2026",
                    TaskUnit = "191 SQN / F-16C", MissionType = "AI", MissionName = "Eagle Strike", Callsign = "EAGLE51", Status = "Active",
                    RawMessage = "MSGID/ATO/HQ AIR/2026-0305//\nTASKUNIT/191 SQN//\nMSNDAT/071000ZMAR2026/AI//\nAMSNLOC/BANDIRMA AB//\nCALLSIGN/EAGLE51//\nIFF-SIF/MODE3A/6722//\nWEAPONS/MK82/6/AIM120C/2//\nTGTDAT/SUPPLY DEPOT/403000N0293000E//\nFREQ/UHF/340.600//"
                },
                new AtoMessage
                {
                    MsgId = "ATO-2026-0306", Dtg = "071500ZMAR2026",
                    TaskUnit = "182 SQN / F-16D", MissionType = "RECCE", MissionName = "Photo Recon", Callsign = "PHOTO61", Status = "Pending",
                    RawMessage = "MSGID/ATO/HQ AIR/2026-0306//\nTASKUNIT/182 SQN//\nMSNDAT/071500ZMAR2026/RECCE//\nAMSNLOC/ERHAC AB//\nCALLSIGN/PHOTO61//\nIFF-SIF/MODE3A/7788//\nWEAPONS/RECCE POD DB-110//\nFREQ/VHF/119.400//"
                },
                new AtoMessage
                {
                    MsgId = "ATO-2026-0307", Dtg = "080600ZMAR2026",
                    TaskUnit = "141 SQN / F-16C BLK40", MissionType = "DCA", MissionName = "Wolf Guard", Callsign = "WOLF71", Status = "Active",
                    RawMessage = "MSGID/ATO/HQ AIR/2026-0307//\nTASKUNIT/141 SQN//\nMSNDAT/080600ZMAR2026/DCA//\nAMSNLOC/ESKISEHIR AB//\nCALLSIGN/WOLF71//\nIFF-SIF/MODE3A/8811//\nWEAPONS/AIM120C/4/AIM9X/2//\nFREQ/UHF/360.200//"
                },
                new AtoMessage
                {
                    MsgId = "ATO-2026-0308", Dtg = "081200ZMAR2026",
                    TaskUnit = "171 SQN / F-4E 2020", MissionType = "SEAD", MissionName = "Phantom Suppression", Callsign = "PHANTOM81", Status = "Parsed",
                    RawMessage = "MSGID/ATO/HQ AIR/2026-0308//\nTASKUNIT/171 SQN//\nMSNDAT/081200ZMAR2026/SEAD//\nAMSNLOC/MALATYA ERHAC AB//\nCALLSIGN/PHANTOM81//\nIFF-SIF/MODE3A/9933//\nWEAPONS/AGM88/2/AIM7M/2//\nTGTDAT/RADAR SITE/380000N0400000E//\nFREQ/UHF/297.500//"
                },
            };

            foreach (AtoMessage message in _allMessages)
            {
                message.StartTime = ParseDtgDateTime(message.Dtg);
                message.EndTime = message.StartTime.AddHours(2);
                message.PublishDate = message.StartTime.AddHours(-12);
            }
        }

        private void ShowOverlay(string text)
        {
            _serviceError = false;
            _statusDetail = text;
            EmptyStatePanel.Visibility = Visibility.Collapsed;
            LoadingText.Text = text;
            ServiceStatusText.Text = "SERVICE STATUS: BUSY";
            ServiceStatusText.Foreground = (Brush)FindResource("ThemeTextMutedBrush");
            SearchOverlay.Visibility = Visibility.Visible;
            ((Storyboard)FindResource("PulseDot1")).Begin();
            ((Storyboard)FindResource("PulseDot2")).Begin();
            ((Storyboard)FindResource("PulseDot3")).Begin();
        }

        private void HideOverlay()
        {
            SearchOverlay.Visibility = Visibility.Collapsed;
            ServiceStatusText.Text = _serviceError ? "SERVICE STATUS: ERROR" : "SERVICE STATUS: READY";
            ServiceStatusText.Foreground = (Brush)FindResource(_serviceError ? "ThemeDangerBrush" : "ThemeTextMutedBrush");
            UpdateHostStatus(_serviceError ? "ERROR" : "READY", _serviceError ? "Error" : _statusDetail);
            ((Storyboard)FindResource("PulseDot1")).Stop();
            ((Storyboard)FindResource("PulseDot2")).Stop();
            ((Storyboard)FindResource("PulseDot3")).Stop();
        }

        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            if (_parseCancellation != null)
                return;

            if (!SearchStartDate.SelectedDate.HasValue || !SearchEndDate.SelectedDate.HasValue)
            {
                DateValidationError.Text = "Start Time and End Time are required.";
                DateValidationError.Visibility = Visibility.Visible;
                return;
            }
            DateValidationError.Visibility = Visibility.Collapsed;

            if (_searchCancellation != null)
                _searchCancellation.Cancel();

            CancellationTokenSource operation = new CancellationTokenSource();
            _searchCancellation = operation;

            DateTime? minPublishDate = SearchMinPublishDate.SelectedDate;
            DateTime? startTime = SearchStartDate.SelectedDate;
            if (startTime.HasValue)
                startTime = startTime.Value.Date;

            DateTime? endTime = SearchEndDate.SelectedDate;
            if (endTime.HasValue)
                endTime = endTime.Value.Date.AddDays(1).AddTicks(-1);

            string missionName = SearchMissionName.Text;

            Stopwatch loadingTimer = Stopwatch.StartNew();
            ShowOverlay("Searching...");

            try
            {
                List<AtoMessage> results = await SearchMessagesAsync(
                    minPublishDate, startTime, endTime, missionName, operation.Token);

                operation.Token.ThrowIfCancellationRequested();
                _filteredMessages.Clear();
                foreach (AtoMessage item in results)
                    _filteredMessages.Add(item);

                ResultCount.Text = string.Format("{0} message(s) found", _filteredMessages.Count);
                _statusDetail = ResultCount.Text;
                UpdateEmptyState(_filteredMessages.Count, minPublishDate, startTime, endTime, missionName);
                await WaitForMinimumLoadingTimeAsync(loadingTimer, operation.Token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _serviceError = true;
                _statusDetail = "Error";
                MessageBox.Show(string.Format("Search error:\n{0}", ex.Message), "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (_searchCancellation == operation)
                {
                    _searchCancellation = null;
                    HideOverlay();
                }
                operation.Dispose();
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            ResetFilters();
        }

        public void ResetFilters()
        {
            if (_searchCancellation != null)
                _searchCancellation.Cancel();

            _serviceError = false;
            _statusDetail = "Ready";
            DateValidationError.Visibility = Visibility.Collapsed;
            ServiceStatusText.Text = "SERVICE STATUS: READY";
            ServiceStatusText.Foreground = (Brush)FindResource("ThemeTextMutedBrush");
            UpdateHostStatus("READY", "Ready");

            SearchMinPublishDate.SelectedDate = null;
            SearchStartDate.SelectedDate = null;
            SearchEndDate.SelectedDate = null;
            SearchMissionName.Text = "";
            _filteredMessages.Clear();
            ResultCount.Text = "";
            EmptyStatePanel.Visibility = Visibility.Collapsed;
        }

        private void SearchStartDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            SearchEndDate.DisplayDateStart = SearchStartDate.SelectedDate;
            if (SearchStartDate.SelectedDate.HasValue && SearchEndDate.SelectedDate.HasValue &&
                SearchEndDate.SelectedDate.Value.Date < SearchStartDate.SelectedDate.Value.Date)
            {
                SearchEndDate.SelectedDate = null;
            }
        }

        private void SearchEndDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            SearchStartDate.DisplayDateEnd = SearchEndDate.SelectedDate;
        }

        private void AtoDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private async void ParseRow_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            AtoMessage msg = btn != null ? btn.Tag as AtoMessage : null;
            if (msg != null)
            {
                if (msg.ParseButtonText != "Parse")
                    return;

                if (!AppSettings.HasOutputFolder)
                {
                    MessageBox.Show("Please select an output folder in Settings before parsing.",
                        "Output Folder Required", MessageBoxButton.OK, MessageBoxImage.Warning);

                    MainWindow mainWin = Window.GetWindow(this) as MainWindow;
                    if (mainWin != null)
                        mainWin.NavigateToSettings();
                    return;
                }

                string folderError;
                if (!AppSettings.TryValidateOutputFolder(AppSettings.OutputFolder, out folderError))
                {
                    MessageBox.Show(folderError, "Output Folder", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string targetPath = Path.Combine(AppSettings.OutputFolder,
                    string.Format("{0}_parsed.txt", msg.MsgId));
                if (File.Exists(targetPath) && MessageBox.Show(
                    string.Format("The output file already exists:\n{0}\n\nOverwrite it?", targetPath),
                    "Confirm Overwrite", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                {
                    return;
                }

                if (_parseCancellation != null)
                    return;

                CancellationTokenSource operation = new CancellationTokenSource();
                _parseCancellation = operation;
                msg.ParseButtonText = "Parsing...";
                Stopwatch loadingTimer = Stopwatch.StartNew();
                ShowOverlay("Parsing...");
                string savedFilePath = null;

                try
                {
                    string filePath = await ParseAndSaveAsync(
                        msg, AppSettings.OutputFolder, operation.Token);
                    operation.Token.ThrowIfCancellationRequested();
                    await WaitForMinimumLoadingTimeAsync(loadingTimer, operation.Token);
                    operation.Token.ThrowIfCancellationRequested();
                    savedFilePath = filePath;
                    msg.Status = "Parsed";
                }
                catch (OperationCanceledException)
                {
                    msg.ParseButtonText = "Parse";
                }
                catch (Exception ex)
                {
                    _serviceError = true;
                    _statusDetail = "Error";
                    msg.ParseButtonText = "Parse";
                    MessageBox.Show(string.Format("Error:\n{0}", ex.Message), "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }

                finally
                {
                    if (_parseCancellation == operation)
                    {
                        _parseCancellation = null;
                        HideOverlay();
                    }
                    operation.Dispose();
                    AtoDataGrid.Items.Refresh();
                }

                if (savedFilePath != null)
                {
                    ResultCount.Text = string.Format("Saved: {0}", Path.GetFileName(savedFilePath));
                    _statusDetail = "Completed";
                    msg.ParseButtonText = "Done";
                    MessageBox.Show(string.Format("Saved to:\n{0}", savedFilePath), "Parse Complete",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    PlayDoneHighlight(btn);
                    await Task.Delay(2000);
                    if (msg.ParseButtonText == "Done")
                        msg.ParseButtonText = "Parse";
                }
            }
        }

        private void PlayDoneHighlight(Button button)
        {
            ScaleTransform scale = new ScaleTransform(1.0, 1.0);
            button.RenderTransformOrigin = new Point(0.5, 0.5);
            button.RenderTransform = scale;

            DoubleAnimation pulse = new DoubleAnimation(1.0, 1.10, TimeSpan.FromMilliseconds(220));
            pulse.AutoReverse = true;
            pulse.RepeatBehavior = new RepeatBehavior(3);
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, pulse);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, pulse.Clone());
        }

        private void UpdateHostStatus(string serviceStatus, string operation)
        {
            MainWindow host = Window.GetWindow(this) as MainWindow;
            if (host != null && host.IsCurrentView(this))
                host.UpdateBottomStatus(serviceStatus, "ATO MESSAGES", operation);
        }

        private void UpdateEmptyState(int resultCount, DateTime? minPublishDate,
            DateTime? startTime, DateTime? endTime, string missionName)
        {
            if (resultCount > 0)
            {
                EmptyStatePanel.Visibility = Visibility.Collapsed;
                return;
            }

            EmptyStateRangeText.Text = string.Format("Date range: {0} - {1}",
                startTime.HasValue ? startTime.Value.ToString("dd.MM.yyyy") : "—",
                endTime.HasValue ? endTime.Value.ToString("dd.MM.yyyy") : "—");

            List<string> filters = new List<string>();
            if (minPublishDate.HasValue)
                filters.Add("Min publish " + minPublishDate.Value.ToString("dd.MM.yyyy"));
            if (!string.IsNullOrWhiteSpace(missionName))
                filters.Add("Mission: " + missionName.Trim());
            EmptyStateFilterText.Text = filters.Count == 0
                ? "Filters: none"
                : "Filters: " + string.Join(", ", filters.ToArray());
            EmptyStatePanel.Visibility = Visibility.Visible;
        }

        private static Task WaitForMinimumLoadingTimeAsync(
            Stopwatch loadingTimer,
            CancellationToken cancellationToken)
        {
            TimeSpan remaining = MinimumLoadingDuration - loadingTimer.Elapsed;
            if (remaining <= TimeSpan.Zero)
                return Task.FromResult(0);

            return Task.Delay(remaining, cancellationToken);
        }

        private Task<List<AtoMessage>> SearchMessagesAsync(
            DateTime? minPublishDate,
            DateTime? startTime,
            DateTime? endTime,
            string missionName,
            CancellationToken cancellationToken)
        {
            // Replace the request body with the generated web-service client call when
            // the external service solution is referenced. The shared policy supplies
            // cancellation, timeout and bounded retry behavior around that call.
            return ServiceRequestPolicy.ExecuteAsync(requestToken => Task.Run(() =>
            {
                requestToken.ThrowIfCancellationRequested();
                IEnumerable<AtoMessage> results = _allMessages;

                if (minPublishDate.HasValue)
                    results = results.Where(m => m.PublishDate.Date >= minPublishDate.Value.Date);
                if (startTime.HasValue)
                    results = results.Where(m => m.StartTime >= startTime.Value);
                if (endTime.HasValue)
                    results = results.Where(m => m.EndTime <= endTime.Value);
                if (!string.IsNullOrWhiteSpace(missionName))
                    results = results.Where(m => m.MissionName.IndexOf(
                        missionName, StringComparison.OrdinalIgnoreCase) >= 0);

                requestToken.ThrowIfCancellationRequested();
                return results.ToList();
            }, requestToken), cancellationToken);
        }

        private Task<string> ParseAndSaveAsync(
            AtoMessage message,
            string outputFolder,
            CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                string parsed = ParseAtoMessage(message.RawMessage);
                string fileName = string.Format("{0}_parsed.txt", message.MsgId);
                string filePath = Path.Combine(outputFolder, fileName);
                cancellationToken.ThrowIfCancellationRequested();
                File.WriteAllText(filePath, parsed);
                return filePath;
            }, cancellationToken);
        }

        private void AtoListView_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_searchCancellation != null)
                _searchCancellation.Cancel();
            if (_parseCancellation != null)
                _parseCancellation.Cancel();
        }

        private DateTime ParseDtgDateTime(string dtg)
        {
            string value = dtg == null ? string.Empty : dtg.Trim().ToUpperInvariant();
            if (value.Length < 14 || value[6] != 'Z')
                throw new FormatException("Invalid DTG value: " + dtg);

            int day;
            int hour;
            int minute;
            int year;
            if (!int.TryParse(value.Substring(0, 2), out day) ||
                !int.TryParse(value.Substring(2, 2), out hour) ||
                !int.TryParse(value.Substring(4, 2), out minute) ||
                !int.TryParse(value.Substring(10, 4), out year))
            {
                throw new FormatException("Invalid DTG value: " + dtg);
            }

            int month;
            switch (value.Substring(7, 3))
            {
                case "JAN": month = 1; break;
                case "FEB": month = 2; break;
                case "MAR": month = 3; break;
                case "APR": month = 4; break;
                case "MAY": month = 5; break;
                case "JUN": month = 6; break;
                case "JUL": month = 7; break;
                case "AUG": month = 8; break;
                case "SEP": month = 9; break;
                case "OCT": month = 10; break;
                case "NOV": month = 11; break;
                case "DEC": month = 12; break;
                default: throw new FormatException("Unknown DTG month: " + value.Substring(7, 3));
            }

            try
            {
                return new DateTime(year, month, day, hour, minute, 0);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                throw new FormatException("Invalid DTG date/time: " + dtg, ex);
            }
        }

        private string ParseAtoMessage(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "Empty message.";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== ATO MESSAGE PARSE RESULT ===");
            sb.AppendLine(string.Format("Parse Date: {0:yyyy-MM-dd HH:mm:ss}", DateTime.Now));
            sb.AppendLine(new string('=', 40));
            sb.AppendLine();

            string[] lines = raw.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                string cleaned = line.TrimEnd('/').Trim();
                string[] parts = cleaned.Split('/');
                if (parts.Length == 0) continue;

                string fieldName = parts[0];
                string fieldValue = string.Join(" / ", parts.Skip(1).Where(p => !string.IsNullOrEmpty(p)));

                string label = fieldName;
                switch (fieldName)
                {
                    case "MSGID": label = "Message ID"; break;
                    case "TASKUNIT": label = "Task Unit"; break;
                    case "MSNDAT": label = "Mission Date/Type"; break;
                    case "AMSNLOC": label = "Departure Base"; break;
                    case "CALLSIGN": label = "Callsign"; break;
                    case "IFF-SIF": label = "IFF/SIF Code"; break;
                    case "WEAPONS": label = "Weapon Load"; break;
                    case "TGTDAT": label = "Target Info"; break;
                    case "FREQ": label = "Frequency"; break;
                    case "AAR": label = "Air-to-Air Refuel"; break;
                }

                sb.AppendLine(string.Format("{0,-20}: {1}", label, fieldValue));
            }

            return sb.ToString();
        }
    }

    public class AtoMessage : INotifyPropertyChanged
    {
        public string MsgId { get; set; }
        public string Dtg { get; set; }
        public string TaskUnit { get; set; }
        public string MissionType { get; set; }
        public string MissionName { get; set; }
        public string Callsign { get; set; }
        public string RawMessage { get; set; }
        public DateTime PublishDate { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public AtoMessage()
        {
            MsgId = string.Empty;
            Dtg = string.Empty;
            TaskUnit = string.Empty;
            MissionType = string.Empty;
            MissionName = string.Empty;
            Callsign = string.Empty;
            RawMessage = string.Empty;
            _status = string.Empty;
            _parseButtonText = "Parse";
        }

        private string _status;
        public string Status
        {
            get { return _status; }
            set { _status = value; OnPropertyChanged(); }
        }

        private string _parseButtonText;
        public string ParseButtonText
        {
            get { return _parseButtonText; }
            set { _parseButtonText = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}
