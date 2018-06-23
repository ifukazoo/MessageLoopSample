using System;
using System.Threading;

namespace MessageLoopSample
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var looper = new ActionLooper())
            {
                looper.Loop();
                int n = 0;
                bool ok = true;
                for (var i = 0; i < 100; i++)
                {
                    var t = new Thread(() =>
                    {
                        while (ok)
                        {
                            Thread.Sleep(1);
                            looper.SendAction(() =>
                            {
                                n++;
                                Console.WriteLine(n);
                            });
                        }
                    });
                    t.Start();
                }
                Console.ReadLine();
                ok = false;
            }
        }
    }
}
