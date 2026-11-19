using System;
using System.IO.Pipes;
using System.Security.Principal;
using System.Threading.Tasks;

namespace DuckDNS2.Ipc
{
    /// <summary>
    /// Client side of the service pipe. Used by the manager UI and by the tray runner (-start).
    /// All calls throw on failure (service not running, timeout, ...).
    /// </summary>
    static class IpcClient
    {
        private const int ConnectTimeoutMs = 2000;
        private const int CallTimeoutMs = 10000;

        public static IpcResponse Call(string command)
        {
            Task<IpcResponse> t = Task.Run(() => CallCore(command));
            if (!t.Wait(CallTimeoutMs))
                throw new TimeoutException("The DuckDNS2 service did not answer in time.");
            return t.Result;
        }

        private static IpcResponse CallCore(string command)
        {
            using (NamedPipeClientStream pipe = new NamedPipeClientStream(".", AppPaths.PipeName,
                PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Identification))
            {
                try
                {
                    pipe.Connect(ConnectTimeoutMs);
                }
                catch (TimeoutException)
                {
                    throw new TimeoutException("Cannot connect to the DuckDNS2 service (is it running?).");
                }
                IpcFraming.Write(pipe, new IpcRequest { Command = command });
                return IpcFraming.Read<IpcResponse>(pipe);
            }
        }

        public static UpdateStatus GetStatus()
        {
            return Unwrap(Call(IpcCommands.Status));
        }

        public static UpdateStatus PushUpdate()
        {
            return Unwrap(Call(IpcCommands.Update));
        }

        public static UpdateStatus Reload()
        {
            return Unwrap(Call(IpcCommands.Reload));
        }

        private static UpdateStatus Unwrap(IpcResponse r)
        {
            if (r == null)
                throw new Exception("Empty answer from the service.");
            if (!r.Success)
                throw new Exception(r.Message ?? "Service returned an error.");
            return r.Status;
        }

        /// <summary>Async wrapper (never blocks the UI thread). Result is null + error on failure.</summary>
        public static Task<Tuple<UpdateStatus, string>> TryAsync(Func<UpdateStatus> call)
        {
            return Task.Run(() =>
            {
                try
                {
                    return Tuple.Create(call(), (string)null);
                }
                catch (AggregateException ae)
                {
                    return Tuple.Create((UpdateStatus)null, ae.GetBaseException().Message);
                }
                catch (Exception ex)
                {
                    return Tuple.Create((UpdateStatus)null, ex.Message);
                }
            });
        }
    }
}
