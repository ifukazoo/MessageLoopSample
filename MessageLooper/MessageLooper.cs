using System;
using System.Threading;
using System.Windows.Forms;

namespace com.yamanobori_old
{
    /// <summary>
    /// メッセージループ
    /// </summary>
    public class MessageLooper :IDisposable
    {
        private Window window = new Window();

        /// <summary>
        /// 生存情報
        /// </summary>
        public bool Alive { get; private set; } = false;
        /// <summary>
        /// ウィンドウハンドル
        /// </summary>
        public IntPtr Handle { get; private set; }
        /// <summary>
        /// メッセージハンドラーデリゲート
        /// </summary>
        /// <param name="msg"></param>
        public delegate void OnMessage(ref Message msg);
        /// <summary>
        /// メッセージループの開始
        /// </summary>
        public void Loop()
        {
            if (Alive) throw new InvalidOperationException("already looped.");

           using (var waitEvent = new ManualResetEvent(false))
            {
                var thread = new Thread(() =>
                {
                    var cp = new CreateParams()
                    {
                        Caption = GetType().FullName
                    };
                    window.CreateHandle(cp);
                    Handle = window.Handle;

                    Alive = true;
                    waitEvent.Set();
                    Application.Run();
                    Alive = false;
                });
                thread.Name = nameof(MessageLooper);
                thread.IsBackground = true;
                thread.Start();
                waitEvent.WaitOne();
            }
        }
        /// <summary>
        /// メッセージループ終了
        /// </summary>
        public void Quit()
        {
            Win32.SendMessage(Handle, Window.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        }
        /// <summary>
        /// メッセージ処理の登録
        /// </summary>
        /// <param name="onMessage"></param>
        public void RegisterHandler(OnMessage onMessage)
        {
            window.OnMessageEvent += onMessage;
        }
        /// <summary>
        /// メッセージ処理の解除
        /// </summary>
        /// <param name="onMessage"></param>
        public void DeregisterHandler(OnMessage onMessage)
        {
            window.OnMessageEvent -= onMessage;
        }
        /// <summary>
        /// <see cref="IDisposable.Dispose"/>
        /// </summary>
        public void Dispose()
        {
            window?.Dispose();
        }
    }

    class Window : NativeWindow, IDisposable
    {
        internal const int WM_CLOSE = 0x0010;

        internal event MessageLooper.OnMessage OnMessageEvent;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_CLOSE)
            {
                Application.ExitThread();
            }
            OnMessageEvent?.Invoke(ref m);
            base.WndProc(ref m);
        }
        public void Dispose() => DestroyHandle();
    }
}
