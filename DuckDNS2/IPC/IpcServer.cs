using System;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

namespace DuckDNS2.Ipc
{
    /// <summary>
    /// Named pipe server hosted inside the Windows service.
    /// Any authenticated local user may connect (the tray runs non-elevated); the
    /// commands exposed are harmless (status / trigger update / reload config from the protected file).
    /// </summary>
    class IpcServer : IDisposable
    {
        private readonly Func<IpcRequest, IpcResponse> handler;
        private Thread thread;
        private volatile bool stopping;
        private readonly ManualResetEvent stopEvent = new ManualResetEvent(false);

        public IpcServer(Func<IpcRequest, IpcResponse> handler)
        {
            this.handler = handler;
        }

        public void Start()
        {
            stopping = false;
            stopEvent.Reset();
            thread = new Thread(ListenLoop) { IsBackground = true, Name = "IPC listener" };
            thread.Start();
        }

        public void Stop()
        {
            stopping = true;
            stopEvent.Set();
            if (thread != null && !thread.Join(3000))
                Log.Error("IPC listener did not stop in time");
            thread = null;
        }

        public void Dispose()
        {
            Stop();
        }

        private static PipeSecurity CreateSecurity()
        {
            PipeSecurity ps = new PipeSecurity();
            ps.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null),
                PipeAccessRights.ReadWrite, AccessControlType.Allow));
            ps.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null),
                PipeAccessRights.FullControl, AccessControlType.Allow));
            ps.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null),
                PipeAccessRights.FullControl, AccessControlType.Allow));
            return ps;
        }

        private void ListenLoop()
        {
            PipeSecurity security = CreateSecurity();
            while (!stopping)
            {
                NamedPipeServerStream pipe = null;
                try
                {
                    pipe = new NamedPipeServerStream(AppPaths.PipeName, PipeDirection.InOut,
                        NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte,
                        PipeOptions.Asynchronous, 4096, 4096, security);

                    // wait for a client OR the stop signal
                    IAsyncResult ar = pipe.BeginWaitForConnection(null, null);
                    if (WaitHandle.WaitAny(new WaitHandle[] { ar.AsyncWaitHandle, stopEvent }) == 1)
                        break; // stopping - pipe is disposed in finally (cancels the pending wait)
                    pipe.EndWaitForConnection(ar);

                    // serve the client on the thread pool, immediately listen for the next one
                    NamedPipeServerStream connected = pipe;
                    pipe = null;
                    ThreadPool.QueueUserWorkItem(_ => Serve(connected));
                }
                catch (Exception ex)
                {
                    if (stopping)
                        break;
                    Log.Error("IPC listen error: " + ex.Message);
                    Thread.Sleep(1000);
                }
                finally
                {
                    if (pipe != null)
                    {
                        try { pipe.Dispose(); } catch { }
                    }
                }
            }
        }

        private void Serve(NamedPipeServerStream pipe)
        {
            using (pipe)
            {
                try
                {
                    IpcRequest req = IpcFraming.Read<IpcRequest>(pipe);
                    IpcResponse resp;
                    try
                    {
                        resp = handler(req);
                    }
                    catch (Exception ex)
                    {
                        resp = new IpcResponse { Success = false, Message = ex.Message };
                    }
                    IpcFraming.Write(pipe, resp);
                    try { pipe.WaitForPipeDrain(); } catch { }
                }
                catch (IOException)
                {
                    // client went away
                }
                catch (Exception ex)
                {
                    Log.Error("IPC serve error: " + ex.Message);
                }
            }
        }
    }
}
