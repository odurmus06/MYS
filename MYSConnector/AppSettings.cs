using System;
using System.IO;

namespace MYSConnector
{
    public static class AppSettings
    {
        public const int DefaultServiceTimeoutSeconds = 30;
        public const int DefaultServiceMaxRetries = 2;
        public const int MinimumServiceTimeoutSeconds = 5;
        public const int MaximumServiceTimeoutSeconds = 300;
        public const int MinimumServiceMaxRetries = 0;
        public const int MaximumServiceMaxRetries = 5;

        private static string _outputFolder = string.Empty;
        private static int _serviceTimeoutSeconds = DefaultServiceTimeoutSeconds;
        private static int _serviceMaxRetries = DefaultServiceMaxRetries;
        private static readonly string _settingsFile = Path.Combine(
            System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
            "MYSConnector", "settings.txt");

        public static string LastError { get; private set; }

        public static string OutputFolder
        {
            get { return _outputFolder; }
            set
            {
                _outputFolder = value;
                string saveError;
                Save(out saveError);
                LastError = saveError;
            }
        }

        public static bool HasOutputFolder
        {
            get { return !string.IsNullOrWhiteSpace(_outputFolder) && Directory.Exists(_outputFolder); }
        }

        public static int ServiceTimeoutSeconds
        {
            get { return _serviceTimeoutSeconds; }
        }

        public static int ServiceMaxRetries
        {
            get { return _serviceMaxRetries; }
        }

        public static void Load()
        {
            LastError = null;
            try
            {
                if (File.Exists(_settingsFile))
                {
                    var lines = File.ReadAllLines(_settingsFile);
                    foreach (var line in lines)
                    {
                        if (line.StartsWith("OutputFolder="))
                            _outputFolder = line.Substring("OutputFolder=".Length);
                        else if (line.StartsWith("ServiceTimeoutSeconds="))
                        {
                            int value;
                            if (int.TryParse(line.Substring("ServiceTimeoutSeconds=".Length), out value) &&
                                value >= MinimumServiceTimeoutSeconds && value <= MaximumServiceTimeoutSeconds)
                                _serviceTimeoutSeconds = value;
                        }
                        else if (line.StartsWith("ServiceMaxRetries="))
                        {
                            int value;
                            if (int.TryParse(line.Substring("ServiceMaxRetries=".Length), out value) &&
                                value >= MinimumServiceMaxRetries && value <= MaximumServiceMaxRetries)
                                _serviceMaxRetries = value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LastError = "Settings could not be read: " + ex.Message;
            }
        }

        public static bool TrySetServicePolicy(int timeoutSeconds, int maxRetries, out string error)
        {
            error = null;
            if (timeoutSeconds < MinimumServiceTimeoutSeconds || timeoutSeconds > MaximumServiceTimeoutSeconds)
            {
                error = string.Format("Timeout must be between {0} and {1} seconds.",
                    MinimumServiceTimeoutSeconds, MaximumServiceTimeoutSeconds);
                LastError = error;
                return false;
            }
            if (maxRetries < MinimumServiceMaxRetries || maxRetries > MaximumServiceMaxRetries)
            {
                error = string.Format("Retry count must be between {0} and {1}.",
                    MinimumServiceMaxRetries, MaximumServiceMaxRetries);
                LastError = error;
                return false;
            }

            int oldTimeout = _serviceTimeoutSeconds;
            int oldRetries = _serviceMaxRetries;
            _serviceTimeoutSeconds = timeoutSeconds;
            _serviceMaxRetries = maxRetries;
            if (!Save(out error))
            {
                _serviceTimeoutSeconds = oldTimeout;
                _serviceMaxRetries = oldRetries;
                LastError = error;
                return false;
            }

            LastError = null;
            return true;
        }

        public static bool TrySetOutputFolder(string folder, out string error)
        {
            if (!TryValidateOutputFolder(folder, out error))
            {
                LastError = error;
                return false;
            }

            _outputFolder = folder;
            if (!Save(out error))
            {
                LastError = error;
                return false;
            }

            LastError = null;
            return true;
        }

        public static bool TryValidateOutputFolder(string folder, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
            {
                error = "The selected output folder does not exist.";
                return false;
            }

            string testFile = Path.Combine(folder, ".mysconnector-write-test-" +
                System.Guid.NewGuid().ToString("N") + ".tmp");
            try
            {
                using (FileStream stream = new FileStream(testFile, FileMode.CreateNew,
                    FileAccess.Write, FileShare.None))
                {
                    stream.WriteByte(0);
                }
                File.Delete(testFile);
                return true;
            }
            catch (Exception ex)
            {
                try
                {
                    if (File.Exists(testFile))
                        File.Delete(testFile);
                }
                catch { }
                error = "The selected output folder is not writable: " + ex.Message;
                return false;
            }
        }

        private static bool Save(out string error)
        {
            error = null;
            try
            {
                var dir = Path.GetDirectoryName(_settingsFile);
                if (dir != null && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllLines(_settingsFile, new[]
                {
                    "OutputFolder=" + _outputFolder,
                    "ServiceTimeoutSeconds=" + _serviceTimeoutSeconds,
                    "ServiceMaxRetries=" + _serviceMaxRetries
                });
                return true;
            }
            catch (Exception ex)
            {
                error = "Settings could not be saved: " + ex.Message;
                return false;
            }
        }
    }
}
