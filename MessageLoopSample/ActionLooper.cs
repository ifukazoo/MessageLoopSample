using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Windows.Forms;

namespace MessageLoopSample
{
    public class ActionLooper : IDisposable
    {
        private Window window = new Window();
        private const int WmAction = 0x8000/*WM_APP*/ + 1;
        private const int WmExit = 0x8000/*  WM_APP*/ + 2;

        private ConcurrentDictionary<int, Action> userActions = new ConcurrentDictionary<int, Action>();
        private int actionId;

        private void handleWmAction(ref Message m)
        {
            var id = m.WParam.ToInt32();
            if (userActions.TryGetValue(id, out Action action))
            {
                action.Invoke();
                while (!userActions.TryRemove(id, out Action discard)) ;
            }
        }

        public void Loop()
        {
            using (var waitEvent = new ManualResetEvent(false))
            {
                var thread = new Thread(() =>
                {
                    var cp = new CreateParams()
                    {
                        Caption = GetType().FullName
                    };
                    window.CreateHandle(cp);
                    window.RegisterHandler(WmAction, handleWmAction);
                    window.RegisterHandler(WmExit, (ref Message m) => Application.ExitThread());
                    Handle = window.Handle;

                    Alive = true;
                    waitEvent.Set();
                    Application.Run();
                    Alive = false;
                });
                thread.Name = nameof(ActionLooper);
                thread.IsBackground = true;
                thread.Start();
                waitEvent.WaitOne();
            }
        }

        public void Quit()
        {
            Win32.SendMessage(Handle, WmExit, IntPtr.Zero, IntPtr.Zero);
        }

        public void SendAction(Action action)
        {
            var id = Interlocked.Increment(ref actionId);
            userActions[id] = action;
            Win32.SendMessage(Handle, WmAction, new IntPtr(id), IntPtr.Zero);
        }
        public bool Alive { get; private set; } = false;
        public IntPtr Handle { get; private set; }

        public void RegisterOnMsg(int wm, OnMessage handler)
        {
            if (wm == WmAction || wm == WmExit) throw new ArgumentException();

            window.RegisterHandler(wm, handler);
        }
        public void UnregiserOnMsg(int wm)
        {
            window.UnregisterHandler(wm);
        }

        #region IDisposable Support
        private bool disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (Alive)
                    {
                        Quit();
                    }
                    window.Dispose();
                }
                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }

    public delegate void OnMessage(ref Message m);

    class Window : NativeWindow, IDisposable
    {
        private ConcurrentDictionary<int, OnMessage> handlers = new ConcurrentDictionary<int, OnMessage>();

        internal void RegisterHandler(int wm, OnMessage handler)
        {
            handlers[wm] = handler;
        }
        internal void UnregisterHandler(int wm)
        {
            handlers[wm] = null;
        }

        protected override void WndProc(ref Message m)
        {
            if (handlers.TryGetValue(m.Msg, out OnMessage handler))
            {
                handler.Invoke(ref m);
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        public void Dispose()
        {
            DestroyHandle();
        }
    }
}
