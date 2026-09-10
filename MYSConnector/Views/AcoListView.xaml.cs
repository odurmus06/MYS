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
    public partial class AcoListView : UserControl
    {
        private List<AcoMessage> _allMessages = new List<AcoMessage>();
        private ObservableCollection<AcoMessage> _filteredMessages = new ObservableCollection<AcoMessage>();
        private CancellationTokenSource _searchCancellation;
        private CancellationTokenSource _parseCancellation;
        private static readonly TimeSpan MinimumLoadingDuration = TimeSpan.FromSeconds(3);
        private bool _serviceError;
        private string _statusDetail = "Ready";

        public AcoListView()
        {
            InitializeComponent();
            LoadDummyData();
            AcoDataGrid.ItemsSource = _filteredMessages;
            Unloaded += AcoListView_Unloaded;
        }

        private void LoadDummyData()
        {
            _allMessages = new List<AcoMessage>
            {
                new AcoMessage { MsgId = "ACO-2026-0501", ValidFrom = "050800ZMAR2026", ValidTo = "060800ZMAR2026", AreaName = "ROZ ALPHA", AcoType = "ROZ", ControlAuth = "CAOC6 ASACS", RawMessage = "MSGID/ACO/CAOC6/2026-0501//\nACOID/ACO PERIOD 05-06 MAR//\nACOAREA/ROZ ALPHA//\nGEOREF/391500N0432000E/392000N0433500E/390500N0433500E/390500N0432000E//\nALTITUDE/FL000/FL250//\nVALID/050800ZMAR2026/060800ZMAR2026//\nUSAGE/RESTRICTED OPERATIONS ZONE//\nCONTROL/CAOC6 ASACS//" },
                new AcoMessage { MsgId = "ACO-2026-0502", ValidFrom = "051400ZMAR2026", ValidTo = "051800ZMAR2026", AreaName = "MRR BRAVO (Corridor)", AcoType = "MRR", ControlAuth = "RAPCON EAST", RawMessage = "MSGID/ACO/CAOC6/2026-0502//\nACOID/ACO PERIOD 05 MAR PM//\nACOAREA/MRR BRAVO//\nCORRIDOR/ENTRY/394000N0280000E/EXIT/400000N0310000E//\nWIDTH/10NM//\nALTITUDE/FL200/FL280//\nVALID/051400ZMAR2026/051800ZMAR2026//\nUSAGE/MINIMUM RISK ROUTE//\nCONTROL/RAPCON EAST//" },
                new AcoMessage { MsgId = "ACO-2026-0503", ValidFrom = "060930ZMAR2026", ValidTo = "061230ZMAR2026", AreaName = "MEZ CHARLIE", AcoType = "MEZ", ControlAuth = "ADOC SOUTH", RawMessage = "MSGID/ACO/CAOC6/2026-0503//\nACOID/ACO PERIOD 06 MAR AM//\nACOAREA/MEZ CHARLIE//\nGEOREF/CENTER/393000N0290000E/RADIUS/30NM//\nALTITUDE/SFC/FL350//\nVALID/060930ZMAR2026/061230ZMAR2026//\nUSAGE/MISSILE ENGAGEMENT ZONE//\nWEAPON_SYS/PATRIOT PAC-3//\nCONTROL/ADOC SOUTH//" },
                new AcoMessage { MsgId = "ACO-2026-0504", ValidFrom = "061245ZMAR2026", ValidTo = "071000ZMAR2026", AreaName = "SHORADEZ DELTA", AcoType = "SHORADS", ControlAuth = "ADOC NORTH", RawMessage = "MSGID/ACO/CAOC6/2026-0504//\nACOID/ACO PERIOD 06-07 MAR//\nACOAREA/SHORADEZ DELTA//\nGEOREF/CENTER/401500N0323000E/RADIUS/15NM//\nALTITUDE/SFC/FL100//\nVALID/061245ZMAR2026/071000ZMAR2026//\nUSAGE/SHORT RANGE AIR DEFENSE ZONE//\nWEAPON_SYS/HAWK/STINGER//\nCONTROL/ADOC NORTH//" },
                new AcoMessage { MsgId = "ACO-2026-0505", ValidFrom = "071000ZMAR2026", ValidTo = "080800ZMAR2026", AreaName = "KILLBOX ECHO", AcoType = "KB", ControlAuth = "ASOC/TACP", RawMessage = "MSGID/ACO/CAOC6/2026-0505//\nACOID/ACO PERIOD 07-08 MAR//\nACOAREA/KILLBOX ECHO//\nGEOREF/391000N0440000E/392500N0443000E/390000N0443000E/390000N0440000E//\nALTITUDE/SFC/UNLIMITED//\nVALID/071000ZMAR2026/080800ZMAR2026//\nUSAGE/CAS KILLBOX//\nCONTROL/ASOC/TACP//" },
                new AcoMessage { MsgId = "ACO-2026-0506", ValidFrom = "080600ZMAR2026", ValidTo = "081800ZMAR2026", AreaName = "ADIZ FOXTROT", AcoType = "ADIZ", ControlAuth = "CAOC6", RawMessage = "MSGID/ACO/CAOC6/2026-0506//\nACOID/ACO PERIOD 08 MAR//\nACOAREA/ADIZ FOXTROT//\nGEOREF/410000N0260000E/420000N0300000E/400000N0300000E/400000N0260000E//\nALTITUDE/SFC/UNLIMITED//\nVALID/080600ZMAR2026/081800ZMAR2026//\nUSAGE/AIR DEFENSE IDENTIFICATION ZONE//\nCONTROL/CAOC6//" },
            };
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

            DateTime? startTime = SearchStartDate.SelectedDate;
            if (startTime.HasValue)
                startTime = startTime.Value.Date;

            DateTime? endTime = SearchEndDate.SelectedDate;
            if (endTime.HasValue)
                endTime = endTime.Value.Date.AddDays(1).AddTicks(-1);

            Stopwatch loadingTimer = Stopwatch.StartNew();
            ShowOverlay("Searching...");

            try
            {
                List<AcoMessage> results = await SearchMessagesAsync(
                    startTime, endTime, operation.Token);

                operation.Token.ThrowIfCancellationRequested();
                _filteredMessages.Clear();
                foreach (AcoMessage item in results)
                    _filteredMessages.Add(item);

                ResultCount.Text = string.Format("{0} message(s) found", _filteredMessages.Count);
                _statusDetail = ResultCount.Text;
                UpdateEmptyState(_filteredMessages.Count, startTime, endTime);
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

            SearchStartDate.SelectedDate = null;
            SearchEndDate.SelectedDate = null;
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

        private void AcoDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private async void ParseRow_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            AcoMessage msg = btn != null ? btn.Tag as AcoMessage : null;
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
                    AcoDataGrid.Items.Refresh();
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
                host.UpdateBottomStatus(serviceStatus, "ACO MESSAGES", operation);
        }

        private void UpdateEmptyState(int resultCount, DateTime? startTime, DateTime? endTime)
        {
            if (resultCount > 0)
            {
                EmptyStatePanel.Visibility = Visibility.Collapsed;
                return;
            }

            EmptyStateRangeText.Text = string.Format("Date range: {0} - {1}",
                startTime.HasValue ? startTime.Value.ToString("dd.MM.yyyy") : "—",
                endTime.HasValue ? endTime.Value.ToString("dd.MM.yyyy") : "—");
            EmptyStateFilterText.Text = "Filters: date range";
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

        private Task<List<AcoMessage>> SearchMessagesAsync(
            DateTime? startTime,
            DateTime? endTime,
            CancellationToken cancellationToken)
        {
            // Replace the request body with the generated web-service client call when
            // the external service solution is referenced. The shared policy supplies
            // cancellation, timeout and bounded retry behavior around that call.
            return ServiceRequestPolicy.ExecuteAsync(requestToken => Task.Run(() =>
            {
                requestToken.ThrowIfCancellationRequested();
                IEnumerable<AcoMessage> results = _allMessages;

                if (startTime.HasValue)
                    results = results.Where(m => ParseDtgDateTime(m.ValidFrom) >= startTime.Value);
                if (endTime.HasValue)
                    results = results.Where(m => ParseDtgDateTime(m.ValidTo) <= endTime.Value);

                requestToken.ThrowIfCancellationRequested();
                return results.ToList();
            }, requestToken), cancellationToken);
        }

        private Task<string> ParseAndSaveAsync(
            AcoMessage message,
            string outputFolder,
            CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                string parsed = ParseAcoMessage(message.RawMessage);
                string fileName = string.Format("{0}_parsed.txt", message.MsgId);
                string filePath = Path.Combine(outputFolder, fileName);
                cancellationToken.ThrowIfCancellationRequested();
                File.WriteAllText(filePath, parsed);
                return filePath;
            }, cancellationToken);
        }

        private void AcoListView_Unloaded(object sender, RoutedEventArgs e)
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

        private string ParseAcoMessage(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "Empty message.";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== ACO MESSAGE PARSE RESULT ===");
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
                    case "ACOID": label = "ACO Period"; break;
                    case "ACOAREA": label = "Area Name"; break;
                    case "GEOREF": label = "Coordinates"; break;
                    case "CORRIDOR": label = "Corridor"; break;
                    case "WIDTH": label = "Width"; break;
                    case "ALTITUDE": label = "Altitude"; break;
                    case "VALID": label = "Validity"; break;
                    case "USAGE": label = "Usage"; break;
                    case "WEAPON_SYS": label = "Weapon System"; break;
                    case "CONTROL": label = "Control Authority"; break;
                }

                sb.AppendLine(string.Format("{0,-20}: {1}", label, fieldValue));
            }

            return sb.ToString();
        }
    }

    public class AcoMessage : INotifyPropertyChanged
    {
        public string MsgId { get; set; }
        public string ValidFrom { get; set; }
        public string ValidTo { get; set; }
        public string AreaName { get; set; }
        public string AcoType { get; set; }
        public string ControlAuth { get; set; }
        public string RawMessage { get; set; }

        public AcoMessage()
        {
            MsgId = string.Empty;
            ValidFrom = string.Empty;
            ValidTo = string.Empty;
            AreaName = string.Empty;
            AcoType = string.Empty;
            ControlAuth = string.Empty;
            RawMessage = string.Empty;
            _parseButtonText = "Parse";
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
