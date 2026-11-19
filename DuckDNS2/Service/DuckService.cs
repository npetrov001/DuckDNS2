using System;
using System.ServiceProcess;

namespace DuckDNS2.Service
{
    /// <summary>Windows service host - started by the SCM when the EXE runs non-interactively.</summary>
    class DuckService : ServiceBase
    {
        private UpdateEngine engine;

        public DuckService()
        {
            ServiceName = AppPaths.ServiceName;
            CanStop = true;
            CanShutdown = true;
            CanPauseAndContinue = false;
            AutoLog = true;
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                AppPaths.EnsureDataDir();
                engine = new UpdateEngine();
                engine.Start();
            }
            catch (Exception ex)
            {
                Log.Error("OnStart error: " + ex);
                throw;
            }
        }

        protected override void OnStop()
        {
            try { engine?.Stop(); }
            catch (Exception ex) { Log.Error("OnStop error: " + ex.Message); }
            engine = null;
        }

        protected override void OnShutdown()
        {
            OnStop();
        }
    }
}
