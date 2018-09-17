using com.yamanobori_old;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace MessageLoopSample
{
    class Program
    {
        static void Main(string[] args)
        {
            const int MSG1 = 0x8001;
            const int MSG2 = 0x8002;
            Action<Message> action = (msg) =>
            {
                var s = Marshal.PtrToStringAnsi(msg.WParam);
                Console.WriteLine($"Thread[{Thread.CurrentThread.ManagedThreadId}] received message.[{s}]");
                Marshal.FreeHGlobal(msg.WParam);
            };

            Console.WriteLine("Message Loop Start.");
            using (var looper1 = new MessageLooper())
            using (var looper2 = new MessageLooper())
            {
                looper1.RegisterHandler((ref Message msg) => { if (msg.Msg == MSG1) action.Invoke(msg); });
                looper2.RegisterHandler((ref Message msg) => { if (msg.Msg == MSG2) action.Invoke(msg); });
                looper1.Loop();
                looper2.Loop();

                for (; ; )
                {
                    var input = Console.ReadLine();
                    input = input.Trim();
                    if (input.Length == 0) continue;
                    if (input.ToUpper().StartsWith("Q"))
                    {
                        looper1.Quit();
                        looper2.Quit();
                        while (looper1.Alive || looper2.Alive)
                            ;
                        break;
                    }

                    var naPtr = Marshal.StringToHGlobalAnsi(input);

                    if (Char.IsLower(input[0]))
                    {
                        Win32.SendMessage(looper1.Handle, MSG1, naPtr, IntPtr.Zero);
                    }
                    else
                    {
                        Win32.SendMessage(looper2.Handle, MSG2, naPtr, IntPtr.Zero);
                    }
                }
            }
            Console.WriteLine("Message Loop End. Hit any key...");
            Console.ReadKey();
        }
    }
}
