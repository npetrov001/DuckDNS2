using System;
using System.Collections.Generic;
using System.Threading;
using DuckDNS2.Ipc;

namespace DuckDNS2.Service
{
    /// <summary>
    /// The heart of the service: loads config, runs DuckDNS updates on a schedule,
    /// keeps the last status and answers IPC requests.
    /// </summary>
    class UpdateEngine : IDisposable
    {
        /// <summary>Delay of the first update after start (network may still be coming up at boot).</summary>
        private static readonly TimeSpan StartupDelay = TimeSpan.FromSeconds(10);
        /// <summary>After a failure retry sooner than the interval (if the interval is longer).</summary>
        private static readonly TimeSpan RetryOnFailure = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan DefaultInterval = TimeSpan.FromMinutes(30);

        private readonly object cfgLock = new object();
        private readonly object updateLock = new object();
        private readonly object statusLock = new object();
        private DDns config = new DDns();
        private readonly UpdateStatus status = new UpdateStatus();
        private Timer timer;
        private IpcServer ipc;
        private volatile bool stopped;
        private int pendingManual; // 1 when a manual update is queued

        public void Start()
        {
            stopped = false;
            lock (statusLock)
            {
                status.ServiceVersion = AppPaths.Version;
                status.ServiceStartedUtc = DateTime.UtcNow;
                status.Messages = new string[0];
            }
            LoadConfig();
            timer = new Timer(_ => RunUpdate("scheduled"), null, Timeout.Infinite, Timeout.Infinite);
            Schedule(StartupDelay);

            ipc = new IpcServer(HandleRequest);
            ipc.Start();
            Log.Info("Engine started v" + AppPaths.Version + ", config: " + AppPaths.ConfigFile);
        }

        public void Stop()
        {
            stopped = true;
            try { ipc?.Stop(); } catch { }
            ipc = null;
            try { timer?.Dispose(); } catch { }
            timer = null;
            Log.Info("Engine stopped");
        }

        public void Dispose()
        {
            Stop();
        }

        private void LoadConfig()
        {
            DDns c = new DDns();
            bool found = c.Load();
            lock (cfgLock)
                config = c;
            lock (statusLock)
            {
                status.Configured = c.IsConfigured;
                status.Domains = c.DomainSummary;
                status.Interval = c.Interval;
            }
            Log.Info(found
                ? "Config loaded. Domains: " + c.DomainSummary + ", interval: " + c.Interval
                : "Config file not found: " + AppPaths.ConfigFile);
        }

        private TimeSpan CurrentInterval()
        {
            lock (cfgLock)
                return DDns.ParseInterval(config.Interval) ?? DefaultInterval;
        }

        private void Schedule(TimeSpan due)
        {
            if (stopped || timer == null)
                return;
            lock (statusLock)
                status.NextUpdateUtc = DateTime.UtcNow + due;
            timer.Change(due, Timeout.InfiniteTimeSpan);
        }

        /// <summary>Runs one update. Serialized - concurrent callers wait for the running one.</summary>
        private void RunUpdate(string reason)
        {
            if (stopped)
                return;
            lock (updateLock)
            {
                if (stopped)
                    return;
                if (reason == "scheduled")
                {
                    // a manual update that ran while we waited for the lock already rescheduled us
                    lock (statusLock)
                        if (status.NextUpdateUtc.HasValue && status.NextUpdateUtc.Value > DateTime.UtcNow.AddSeconds(2))
                            return;
                }
                else
                    Interlocked.Exchange(ref pendingManual, 0);

                DDns cfg;
                lock (cfgLock)
                    cfg = config;

                if (!cfg.IsConfigured)
                {
                    lock (statusLock)
                    {
                        status.Configured = false;
                        status.NextUpdateUtc = null;
                    }
                    Log.Info("Update skipped (" + reason + "): not configured");
                    // check again later in case config was dropped in without a RELOAD
                    Schedule(CurrentInterval());
                    return;
                }

                lock (statusLock)
                    status.InProgress = true;

                List<string> msgs = new List<string>();
                try
                {
                    cfg.Update(msgs);
                }
                catch (Exception ex)
                {
                    msgs.Add(ex.GetType().Name + ": " + ex.Message);
                }

                bool ok = msgs.Count == 0;
                DateTime now = DateTime.UtcNow;
                lock (statusLock)
                {
                    status.InProgress = false;
                    status.Configured = true;
                    status.LastAttemptUtc = now;
                    status.LastResult = ok ? UpdateResult.Success : UpdateResult.Failed;
                    status.Messages = msgs.ToArray();
                    status.UpdateCount++;
                    if (ok)
                        status.LastSuccessUtc = now;
                    else
                        status.FailureCount++;
                }

                if (ok)
                    Log.Info("Update OK (" + reason + "): " + cfg.DomainSummary);
                else
                    Log.Error("Update FAILED (" + reason + "): " + string.Join(" | ", msgs));

                TimeSpan next = CurrentInterval();
                if (!ok && next > RetryOnFailure)
                    next = RetryOnFailure;
                Schedule(next);
            }
        }

        /// <summary>Queues an immediate update on the thread pool (coalesces repeated requests).</summary>
        public void TriggerUpdate(string reason)
        {
            if (Interlocked.Exchange(ref pendingManual, 1) == 1)
                return; // already queued
            lock (statusLock)
            {
                if (status.Configured)
                    status.InProgress = true; // reflect immediately for pollers
            }
            ThreadPool.QueueUserWorkItem(_ => RunUpdate(reason));
        }

        public UpdateStatus GetStatus()
        {
            lock (statusLock)
                return status.Clone();
        }

        private IpcResponse HandleRequest(IpcRequest req)
        {
            string cmd = (req?.Command ?? "").Trim().ToUpperInvariant();
            switch (cmd)
            {
                case IpcCommands.Status:
                    break;
                case IpcCommands.Update:
                    TriggerUpdate("manual");
                    break;
                case IpcCommands.Reload:
                    LoadConfig();
                    TriggerUpdate("config reload");
                    break;
                default:
                    return new IpcResponse { Success = false, Message = "Unknown command '" + cmd + "'" };
            }
            return new IpcResponse { Success = true, Message = "OK", Status = GetStatus() };
        }
    }
}
