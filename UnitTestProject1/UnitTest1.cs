using System;
using System.Threading;
using MessageLoopSample;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        private ActionLooper looper;

        [TestInitialize]
        public void Setup()
        {
            looper = new ActionLooper();
        }

        [TestMethod]
        public void TestMethod1()
        {
            looper.Loop();
            bool testvalue = false;
            looper.SendAction(() => testvalue = true);
            Assert.AreEqual(true, testvalue);
        }
        [TestMethod]
        public void TestMethod11()
        {
            looper.Loop();
            bool v1, v2, v3;
            v1 = v2 = v3 = false;
            var t1 = new Thread(() =>
            {
                looper.SendAction(() => v1 = true);
            });
            var t2 = new Thread(() =>
            {
                looper.SendAction(() => v2 = true);
            });
            var t3 = new Thread(() =>
            {
                looper.SendAction(() => v3 = true);
            });
            t1.Start();
            t2.Start();
            t3.Start();
            t1.Join();
            t2.Join();
            t3.Join();
            Assert.AreEqual(true, v1);
            Assert.AreEqual(true, v2);
            Assert.AreEqual(true, v3);
        }
        [TestMethod]
        public void TestMethod2()
        {
            looper.Loop();
            Assert.AreEqual(true, looper.Alive);
        }
        [TestMethod]
        public void TestMethod3()
        {
            Assert.AreEqual(false, looper.Alive);
            looper.Loop();
            Assert.AreEqual(true, looper.Alive);
            looper.Quit();
            Thread.Sleep(10);
            Assert.AreEqual(false, looper.Alive);
        }
        [TestMethod]
        public void TestMethod4()
        {
            looper.Loop();
            looper.Dispose();
            Assert.AreEqual(false, looper.Alive);
        }
        [TestMethod]
        public void TestMethod5()
        {
            looper.Dispose();
        }
        [TestMethod]
        public void TestMethod6()
        {
            looper.Quit();
        }
        [TestMethod]
        public void TestMethod7()
        {
            bool testvalue = false;
            looper.SendAction(() => testvalue = true);
            Assert.AreEqual(false, testvalue);
        }
        [TestMethod]
        public void TestMethod8()
        {
            looper.Loop();
            Assert.AreEqual(false, looper.Busy);
            bool busy = false;
            new Thread(() =>
            {
                Thread.Sleep(100);
                busy = looper.Busy;

            }).Start();
            looper.SendAction(() =>
            {
                Thread.Sleep(200);
            });
            Assert.AreEqual(true, busy);
            Assert.AreEqual(false, looper.Busy);
        }
        [TestMethod]
        public void TestMethod9()
        {
            const int WM_TEST = 0x8000 + 3;
            var expect = new IntPtr(1);
            IntPtr actual = IntPtr.Zero;
            looper.RegisterOnMsg(WM_TEST, (ref System.Windows.Forms.Message m) => {
                actual = m.WParam;
            });
            looper.Loop();
            Win32.SendMessage(looper.Handle, WM_TEST, expect, IntPtr.Zero);
            Assert.AreEqual(expect, actual);
        }
        [TestMethod]
        public void TestMethod10()
        {
            const int WM_TEST = 0x8000 + 3;
            IntPtr actual = IntPtr.Zero;
            var expect = new IntPtr(1);
            looper.RegisterOnMsg(WM_TEST, (ref System.Windows.Forms.Message m) =>
            {
                actual = m.WParam;
            });
            looper.Loop();
            Win32.SendMessage(looper.Handle, WM_TEST, expect, IntPtr.Zero);
            Assert.AreEqual(expect, actual);

            looper.UnregiserOnMsg(WM_TEST);
            actual = IntPtr.Zero;
            Win32.SendMessage(looper.Handle, WM_TEST, expect, IntPtr.Zero);
            Assert.AreNotEqual(expect, actual);
        }
    }
}
