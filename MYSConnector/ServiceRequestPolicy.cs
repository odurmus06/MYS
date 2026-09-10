using System;
using System.Threading;
using System.Threading.Tasks;

namespace MYSConnector
{
    public static class ServiceRequestPolicy
    {
        public const int TimeoutSeconds = AppSettings.DefaultServiceTimeoutSeconds;
        public const int MaxRetries = AppSettings.DefaultServiceMaxRetries;
        public const int RetryDelayMilliseconds = 500;

        public static async Task<T> ExecuteAsync<T>(
            Func<CancellationToken, Task<T>> request,
            CancellationToken cancellationToken)
        {
            Exception lastError = null;
            int timeoutSeconds = AppSettings.ServiceTimeoutSeconds;
            int maxRetries = AppSettings.ServiceMaxRetries;
            for (int attempt = 0; attempt <= maxRetries; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                using (CancellationTokenSource timeout =
                    CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
                {
                    timeout.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));
                    try
                    {
                        return await request(timeout.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        if (cancellationToken.IsCancellationRequested)
                            throw;
                        lastError = new TimeoutException(
                            "The service request timed out after " + timeoutSeconds + " seconds.");
                    }
                    catch (Exception ex)
                    {
                        lastError = ex;
                    }
                }

                if (attempt < maxRetries)
                    await Task.Delay(RetryDelayMilliseconds * (attempt + 1), cancellationToken);
            }

            throw lastError ?? new InvalidOperationException("The service request failed.");
        }
    }
}
